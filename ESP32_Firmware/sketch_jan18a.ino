#include <WiFi.h>
#include <ModbusIP_ESP8266.h>
#include "DHT.h"

// --- 1. WI-FI SETTINGS ---
const char* ssid = "TP-Link_F94E";
const char* password = "93250238";

// --- 2. SENSOR SETTINGS ---
#define DHTPIN 4
#define DHTTYPE DHT11
DHT dht(DHTPIN, DHTTYPE);

// --- 3. MODBUS SETTINGS ---
ModbusIP mb; // Create the Modbus Object

// We will store temperature in Register 0
// (PLCs usually call this "Holding Register 40001" or just "0")
const int TEMP_REGISTER = 0;

void setup() {
  Serial.begin(115200);
  dht.begin();

  // Connect to Wi-Fi
  Serial.print("Connecting to Wi-Fi");
  WiFi.begin(ssid, password);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println(" Connected!");
  Serial.print("ESP32 IP Address: ");
  Serial.println(WiFi.localIP()); // <--- IMPORTANT: Write this down!

  // Configure ESP32 as a Modbus Server (Slave)
  mb.server();
  
  // Create the register 0 and set initial value to 0
  mb.addHreg(TEMP_REGISTER, 0); 
}

void loop() {
  // Talk to Modbus Clients (Node-RED, PLC, etc.)
  mb.task();

  // Update the data every 2 seconds
  static unsigned long lastUpdate = 0;
  if (millis() - lastUpdate > 2000) {
    lastUpdate = millis();

    // Read Sensor
    float t = dht.readTemperature();
    
    // Error check
    if (isnan(t)) {
      t = 0.0; 
    }

    // Convert to Integer (e.g. 24.5 C becomes 245)
    uint16_t rawTemp = (uint16_t)(t * 10);

    // Update the Modbus Register
    mb.Hreg(TEMP_REGISTER, rawTemp);

    Serial.print("Temperature in Register 0: ");
    Serial.println(rawTemp);
  }
}