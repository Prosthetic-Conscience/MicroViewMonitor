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
using OpenHardwareMonitor;
using OpenHardwareMonitor.Hardware;

namespace MicroviewMonitor
    {
    public partial class MonitorWindow : Form
        {
        Computer thisComputer;


        public MonitorWindow()
            {
            InitializeComponent();
            thisComputer = new Computer() { CPUEnabled = true };
            thisComputer.Open();
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
            this.ShowInTaskbar = true;
            //myForm.ShowInTaskbar = false;
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

        

        private void timer_Tick(object sender, EventArgs e)
            {
            List<float> cpuAvgtemp = new List<float>();
            String load = "";
            if (serialPort1.IsOpen)
                {
                foreach (var hardwareItem in thisComputer.Hardware)
                    {
                    if (hardwareItem.HardwareType == HardwareType.CPU)
                        {
                        hardwareItem.Update();
                        foreach (IHardware subHardware in hardwareItem.SubHardware)
                        subHardware.Update();

                        foreach (var sensor in hardwareItem.Sensors)
                            {
                            if (sensor.SensorType == SensorType.Temperature)
                                {
                                cpuAvgtemp.Add(sensor.Value.Value);
                                //temp += String.Format("{0} Temperature = {1}\r\n", sensor.Name, sensor.Value.HasValue ? sensor.Value.Value.ToString() : "no value");
                                }
                            if (sensor.SensorType == SensorType.Load  && sensor.Name == "CPU Total")
                                {
                                //load += String.Format("{0} Load = {1}\r\n", sensor.Name, sensor.Value.HasValue ? sensor.Value.Value.ToString() : "no value");
                                load += String.Format(sensor.Value.Value.ToString());
                                }
                            }
                        }
                    }               
                System.Diagnostics.Debug.WriteLine("CPU Load " + load);
                if (cbFahrenheit.Checked == true)
                    {
                    double dbCpuAvgtemp = (((9.0 / 5.0) * cpuAvgtemp.Average()) + 32.0);
                    serialPort1.WriteLine(dbCpuAvgtemp.ToString() + ":" + load);
                    System.Diagnostics.Debug.WriteLine("CPU Average temp " + dbCpuAvgtemp.ToString());
                    }
                else
                    {
                    serialPort1.WriteLine(cpuAvgtemp.Average().ToString() + ":" + load);
                    System.Diagnostics.Debug.WriteLine("CPU Average temp " + cpuAvgtemp.Average().ToString());
                    }
                System.Diagnostics.Debug.WriteLine("CPU Load " + load);
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
