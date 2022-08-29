using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Management;
using OpenHardwareMonitor.Hardware;

namespace MicroviewMonitor
    {
    public partial class MonitorWindow : Form
        {
        Boolean bAHCPI_Available;
        Computer myComputer;


        public MonitorWindow()
            {
            InitializeComponent();
            bAHCPI_Available = test_For_AHCPI();
            }

        private void cbPortName_DropDown(object sender, EventArgs e)
            {
            // Get all serial ports name
            string[] ports = SerialPort.GetPortNames();

            // Clear comboBox list to get latest serial ports in each dropdown.
            cbPortName.Items.Clear();
            foreach (string port in ports)
                {
                // Output serial port to dropdown list.
                cbPortName.Items.Add(port);
                }
            }

        private void MonitorWindow_Resize(object sender, EventArgs e)
            {
            if (this.WindowState == FormWindowState.Minimized)
                {
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(500);
                this.ShowInTaskbar = false;
                this.Hide();
                }
            }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
            {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
            }

        private void btn_Start_Click(object sender, EventArgs e)
            {
            if (serialPort1.IsOpen)
                {
                serialPort1.Close();
                }
            if (cbPortName.Text != null && cbPortName.Text != "")
                {
                serialPort1.PortName = cbPortName.Text; //cbPortname.Text set to PortName
                serialPort1.BaudRate = 9600; //Baudrate is fixed
                serialPort1.DataBits = 8; //Databits fixed
                serialPort1.Parity = Parity.None; //Parity fixed
                serialPort1.StopBits = StopBits.One; //Stop bits fixed

                //If port isn't open, then open it
                if (!serialPort1.IsOpen)
                    {
                    serialPort1.Open();
                    }
                //Enable timer
                timer.Enabled = true;
                }
            if (cbPortName.Text == null || cbPortName.Text == "")
                {
                MessageBox.Show("Must select a serial port");
                }
            }

        private string get_CPU_Load()
            {
            //Get CPU usage values using a WMI query
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor");
            var cpuTimes = searcher.Get()
                .Cast<ManagementObject>()
                .Select(mo => new
                    {
                    Name = mo["Name"],
                    Usage = mo["PercentProcessorTime"]
                    }
                )
                .ToList();
            //The '_Total' value represents the average usage across all cores,
            //and is the best representation of overall CPU usage
            var query = cpuTimes.Where(x => x.Name.ToString() == "_Total").Select(x => x.Usage);
            var cpuUsage = query.SingleOrDefault();
            return cpuUsage.ToString();
            }

        private Boolean test_For_AHCPI()
            {
            ManagementObjectSearcher searcherCPU = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor");
            ManagementObjectSearcher searcherLoad = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor");
            //MessageBox.Show("Found: " + searcher.Get().Count.ToString());
            if ((searcherCPU.Get().Count > 0) && (searcherLoad.Get().Count > 0))
                {
                return true;
                }
            else
                {
                return false;
                }
            }


        private string get_CPU_Temp_AHCPI()
            {
            List<Double> result = new List<Double>();
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
            foreach (ManagementObject obj in searcher.Get())
                {
                Double temp = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                temp = ((temp - 2732) / 10);
                temp = ((9.0 / 5.0) * temp) + 32;
                result.Add(temp);
                }
            return Convert.ToInt32(result.Max()).ToString();            
            //return result.Max().ToString();
            }

        private void timer_Tick(object sender, EventArgs e)
            {
            //Test to see if CPU temp available using AHCPI
            //if(bAHCPI_Available == true)
            //{
            string currentTemp = get_CPU_Temp_AHCPI();
            //}
            // else if bAHCPI_Available == false)
            //{
            //
            //}
            string currentLoad = get_CPU_Load();
            //MessageBox.Show(currentTemp);
            //Console.WriteLine(currentTemp);
            if (serialPort1.IsOpen)
                {
                System.Diagnostics.Debug.WriteLine("Temp " + currentTemp + "F : " + "Current Load " +currentLoad + "%");
                serialPort1.WriteLine(currentTemp + ":" + currentLoad);
                }
            if (!serialPort1.IsOpen)
                {
                timer.Enabled = false;
                MessageBox.Show("Serial port is closed");
                }
            }

        private void btn_Stop_Click(object sender, EventArgs e)
            {
            timer.Enabled = false;
            }



        }
    }
