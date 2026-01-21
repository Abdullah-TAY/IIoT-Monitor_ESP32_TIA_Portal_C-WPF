using S7.Net; // Standard S7 Driver
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfPlcMonitor
{
    public partial class MainWindow : Window
    {
        private Plc _plc;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Configure the PLC Connection
                // Change "192.168.0.10" to your actual PLCSIM IP
                _plc = new Plc(CpuType.S71500, "192.168.0.10", 0, 1);

                _plc.Open();

                if (_plc.IsConnected)
                {
                    TxtStatus.Text = "Connected to PLC (DB2)!";
                    TxtStatus.Foreground = Brushes.Green;
                    BtnConnect.IsEnabled = false;

                    // Start Timer
                    _timer = new DispatcherTimer();
                    _timer.Interval = TimeSpan.FromMilliseconds(500);
                    _timer.Tick += Timer_Tick;
                    _timer.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection Error: " + ex.Message);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_plc != null && _plc.IsConnected)
            {
                try
                {
                    // ---------------------------------------------------------
                    // READING THE DATA (The Fix)
                    // ---------------------------------------------------------
                    // 1. DB Number: 2 (As you requested)
                    // 2. Start Byte: 66 (REPLACE THIS WITH YOUR TIA PORTAL OFFSET!)
                    // 3. VarType: Real (Float) - No more "Word" or "ushort"

                    // We cast directly to float because the PLC is giving us a Real number.
                    float finalTemp = (float)_plc.Read(DataType.DataBlock, 2, 22, VarType.Real, 1);

                    // ---------------------------------------------------------
                    // UPDATING UI (No Math Needed!)
                    // ---------------------------------------------------------

                    // Update Text
                    TxtTemp.Content = finalTemp.ToString("0.0") + " °C";

                    // Update Progress Bar
                    PbTemp.Value = finalTemp;

                    // Color Logic
                    if (finalTemp > 28.0)
                        TxtTemp.Foreground = Brushes.Red;
                    else
                        TxtTemp.Foreground = Brushes.Black;
                }
                catch (Exception)
                {
                    _timer.Stop();
                    TxtStatus.Text = "Read Failed";
                    TxtStatus.Foreground = Brushes.Red;
                    BtnConnect.IsEnabled = true;
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _plc?.Close();
            base.OnClosed(e);
        }
    }
}