using LibreHardwareMonitor.Hardware;
using Microsoft.Win32.TaskScheduler;
using System.Diagnostics;
using System.IO.Ports;





namespace PCSD_Control_Panel_2._0
{
    public partial class Form1 : Form
    {
        // top bar
        bool dragging = false;
        Point offset;
        Point original;

        // LibreHardwareMonitor
        private static Computer myComputer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true
        };

        // CPU/GPU temperature/usage
        private static int cpuTemp;
        private static int gpuTemp;
        private static int cpuUsage;
        private static int gpuUsage;
        private static int ramUsed;
        private static int ramTotal;

        // USB Display
        private object[] buadRateSet = { 300, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 74880, 115200, 230400, 250000, 500000, 1000000, 2000000 };
        private static SerialPort port = new SerialPort();


        //task service
        private static string folderName = Application.ProductName + "";
        private static string programPath = Application.ExecutablePath.ToString();
        private static string taskName = Application.ProductName + " startup";


        //init
        public Form1()
        {
            InitializeComponent();

            // Initialize computer object and enable CPU and GPU sensors
            myComputer.Open();

            //start to get status
            getStatus.IsBackground = true;
            getStatus.Start();

            //init timer
            timer1 = new System.Windows.Forms.Timer();
            timer1.Interval = 250;
            timer1.Tick += new EventHandler(timer1_Tick);
            timer2 = new System.Windows.Forms.Timer();
            timer2.Interval = 100;
            timer2.Tick += new EventHandler(timer2_Tick);

            //init USB display
            comboBox1.Items.AddRange(getPortName());
            comboBox2.Items.AddRange(buadRateSet);

            //notify icon
            notifyIcon1.Icon = Properties.Resources.PCSD_Control_Panel;
            notifyIcon1.Text = Properties.Settings.Default.appName;
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Visible = true;

            //form
            this.Text = Properties.Settings.Default.appName;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //form setup
            panelSystem.Visible = true;
            panelSetting.Visible = false;
            panelUSBDisplay.Visible = false;

            //start timer
            timer1.Start();
            timer2.Start();

            //get value from properties settings
            numericUpDown1.Value = Properties.Settings.Default.update_speed;
            comboBox1.SelectedIndex = comboBox1.Items.IndexOf(Properties.Settings.Default.port);
            comboBox2.SelectedIndex = comboBox2.Items.IndexOf((int)Properties.Settings.Default.buad_rate);

            //icon
            this.Icon = Properties.Resources.PCSD_Control_Panel; // Set the form icon
            notifyIcon1.Icon = Properties.Resources.PCSD_Control_Panel; // Set the notify icon

            //start sending data to USB display
            if (Properties.Settings.Default.status)
            {
                Thread sendDataThread = new Thread(sendStatus);
                sendDataThread.IsBackground = true;
                sendDataThread.Start();
            }

            //create a folder in the root task folder
            CreateFolder(folderName);

        }
        private void Form1_Shown(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.status)
            {
                this.Visible = false;
            }
        }

        // Create a folder in windows task scheduler if there don't have
        static void CreateFolder(string folderName)
        {
            using (TaskService ts = new TaskService())
            {
                // Check if the folder already exists
                if (ts.GetFolder("\\" + folderName) == null)
                {
                    // Create the folder
                    ts.RootFolder.CreateFolder(folderName);
                    Console.WriteLine("Folder created: " + folderName);
                }
                else
                {
                    Console.WriteLine("Folder already exists: " + folderName);
                }
            }
        }

        // Create a task in this folder to make its program run in startup with administrator permission,if there don't have task
        static void CreateTask(string folderName, string taskName, string programPath)
        {
            using (TaskService ts = new TaskService())
            {
                // Get the folder
                TaskFolder folder = ts.GetFolder("\\" + folderName);
                if (folder != null)
                {
                    // Check if the task already exists
                    if (folder.GetTasks().FirstOrDefault(tt => tt.Name == taskName) == null)
                    {
                        // Create a new task definition
                        TaskDefinition td = ts.NewTask();
                        td.RegistrationInfo.Description = "Run a program at startup with administrator permission";

                        // Set the trigger to run at startup
                        td.Triggers.Add(new LogonTrigger());

                        // Set the action to run the program
                        td.Actions.Add(new ExecAction(programPath));

                        // Set the task to run as administrator
                        td.Principal.RunLevel = TaskRunLevel.Highest;

                        // Register the task in the folder
                        folder.RegisterTaskDefinition(taskName, td);
                        Console.WriteLine("Task created: " + taskName);
                    }
                    else
                    {
                        Console.WriteLine("Task already exists: " + taskName);
                    }
                }
                else
                {
                    Console.WriteLine("Folder not found: " + folderName);
                }
            }
        }
        // Delete the task, if there have task
        static void DeleteTask(string folderName, string taskName)
        {
            using (TaskService ts = new TaskService())
            {
                // Get the folder
                TaskFolder folder = ts.GetFolder("\\" + folderName);
                if (folder != null)
                {
                    // Check if the task exists
                    if (folder.GetTasks().FirstOrDefault(tt => tt.Name == taskName) != null)
                    {
                        // Delete the task
                        folder.DeleteTask(taskName);
                        Console.WriteLine("Task deleted: " + taskName);
                    }
                    else
                    {
                        Console.WriteLine("Task not found: " + taskName);
                    }
                }
                else
                {
                    Console.WriteLine("Folder not found: " + folderName);
                }
            }
        }


        private object[] getPortName()
        {
            SerialPort detectPort = new SerialPort();
            string[] ports = SerialPort.GetPortNames();
            return ports;
        }

        // Close button
        private void button2_Click(object sender, EventArgs e)
        {
            // Close the form
            this.Visible = false;
            timer1.Stop();
        }

        // Top bar
        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            // Set the dragging flag to true and capture the mouse
            dragging = true;
            panelTopBar.Capture = true;

            // Store the current mouse position and the original form location
            offset = MousePosition;
            original = this.Location;
        }
        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            // Check if the dragging flag is true
            if (dragging)
            {
                // Calculate the new form location based on the mouse position and the offset
                int x = original.X + MousePosition.X - offset.X;
                int y = original.Y + MousePosition.Y - offset.Y;

                // Set the new form location
                this.Location = new Point(x, y);
            }
        }
        private void panel2_MouseUp(object sender, MouseEventArgs e)
        {
            // Set the dragging flag to false and release the mouse
            dragging = false;
            panelTopBar.Capture = false;
        }

        // Get status
        Thread getStatus = new Thread(delegate ()
        {
            try
            {
                int cpuTempTmp, cpuUsageTmp, gpuTempTmp, gpuUsageTmp;
                while (true)
                {
                    // Get the current values of CPU and GPU temperature and usage
                    cpuTempTmp = GetCPUTemp();
                    gpuTempTmp = GetGPUTemp();
                    cpuUsageTmp = GetCPUUsage();
                    gpuUsageTmp = GetGPUUsage();
                    cpuTemp = cpuTempTmp;
                    gpuTemp = gpuTempTmp;
                    cpuUsage = cpuUsageTmp;
                    gpuUsage = gpuUsageTmp;
                    UpdateRam();
                    Thread.Sleep(Properties.Settings.Default.update_speed);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error (getStatus)", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        });

        // Send status
        static public void sendStatus()
        {
            try
            {
                port.Close();
                port.BaudRate = Properties.Settings.Default.buad_rate;
                port.PortName = Properties.Settings.Default.port;
                port.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Properties.Settings.Default.status = false;
                Properties.Settings.Default.Save();
            }

            while (Properties.Settings.Default.status)
            {
                try
                {
                    string cpuTempText = cpuTemp.ToString("D3");
                    string cpuUsageText = cpuUsage.ToString("D3");
                    string gpuTempText = gpuTemp.ToString("D3");
                    string gpuUsageText = gpuUsage.ToString("D3");
                    string ramUsedText = ramUsed.ToString("D5");
                    string ramTotalText = ramTotal.ToString("D5");
                    port.Write(cpuTempText + cpuUsageText + gpuTempText + gpuUsageText + ramUsedText + ramTotalText);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Properties.Settings.Default.status = false;
                    Properties.Settings.Default.Save();
                }
                Thread.Sleep(Properties.Settings.Default.update_speed);
            }
            port.Close();
        }
        // Get value
        private static int GetCPUTemp()
        {
            foreach (var hardware in myComputer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    hardware.Update();
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature &&
                            sensor.Name == "Core (Tctl/Tdie)" &&
                            sensor.Value.HasValue)
                        {
                            return (int)Math.Round(sensor.Value.Value);
                        }
                    }
                }
            }
            return 0;
        }
        private static int GetGPUTemp()
        {
            foreach (var hardware in myComputer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                {
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.Name == "GPU Core" && sensor.SensorType == SensorType.Temperature)
                        {
                            hardware.Update();
                            if (sensor.Value != null)
                            {
                                return ((int)sensor.Value);
                            }
                        }
                    }
                }
            }
            return 0;
        }
        private static int GetCPUUsage()
        {
            foreach (var hardware in myComputer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.Name == "CPU Total" && sensor.SensorType == SensorType.Load)
                        {
                            hardware.Update();
                            if (sensor.Value != null)
                            {
                                return ((int)sensor.Value);
                            }
                        }
                    }
                }
            }
            return 0;
        }
        private static int GetGPUUsage()
        {
            foreach (var hardware in myComputer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                {
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.Name == "GPU Core" && sensor.SensorType == SensorType.Load)
                        {
                            hardware.Update();
                            if (sensor.Value != null)
                            {
                                return ((int)sensor.Value);
                            }
                        }
                    }
                }
            }
            return 0;
        }
        private static void UpdateRam()
        {
            foreach (var hardware in myComputer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Memory)
                {
                    hardware.Update();
                    float? used = null;
                    float? available = null;
                    foreach (var sensor in hardware.Sensors)
                    {
                        if (sensor.Name == "Memory Used" && sensor.Value.HasValue)
                            used = sensor.Value;
                        if (sensor.Name == "Memory Available" && sensor.Value.HasValue)
                            available = sensor.Value;
                    }
                    if (used != null && available != null)
                    {
                        ramUsed = (int)used.Value;
                        ramTotal = (int)(used.Value + available.Value);
                    }
                }
            }
        }
        //menu button
        private void button3_Click(object sender, EventArgs e)
        {
            panelSystem.Visible = false;
            panelSetting.Visible = false;
            panelUSBDisplay.Visible = true;
            timer1.Stop();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            panelSystem.Visible = false;
            panelSetting.Visible = true;
            panelUSBDisplay.Visible = false;
            timer1.Stop();

        }
        private void button1_Click(object sender, EventArgs e)
        {
            panelSystem.Visible = true;
            panelSetting.Visible = false;
            panelUSBDisplay.Visible = false;
            timer1.Start();
        }

        //console app button
        private void button5_Click(object sender, EventArgs e)
        {
            Process.Start("Console.exe");
        }
        // update static in system page
        private void timer1_Tick(object? sender, EventArgs e)
        {

            label5.Text = $"{cpuTemp} �C";
            label6.Text = $"{gpuTemp} �C";
            label8.Text = $"{cpuUsage} %";
            label7.Text = $"{gpuUsage} %";
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.update_speed = Convert.ToInt32(numericUpDown1.Value);
            Properties.Settings.Default.Save();
        }


        // button to run or stop the USB display
        private void button8_Click(object sender, EventArgs e)
        {
            if (!Properties.Settings.Default.status)
            {
                Thread sendDataThread = new Thread(sendStatus);
                sendDataThread.IsBackground = true;
                sendDataThread.Start();
            }
            Properties.Settings.Default.status = !Properties.Settings.Default.status;
            Properties.Settings.Default.Save();
        }
        // button to apply USB display setting
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                Properties.Settings.Default.port = comboBox1.Text;
                Properties.Settings.Default.buad_rate = Convert.ToInt32(comboBox2.Text);
                port.Close();
                port.BaudRate = Properties.Settings.Default.buad_rate;
                port.PortName = Properties.Settings.Default.port;
                port.Open();
                if (Properties.Settings.Default.status)
                {
                    Properties.Settings.Default.status = false;
                }
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        // update port selection when click the comboBox
        private void comboBox1_MouseClick(object sender, MouseEventArgs e)
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(getPortName());
        }

        // keep checking
        private void timer2_Tick(object? sender, EventArgs e)
        {
            if (Properties.Settings.Default.status)
            {
                button8.Text = "Stop";
                label14.Text = "active";
                label14.ForeColor = Color.Lime;
                button7.Enabled = false;
                comboBox1.Enabled = false;
                comboBox2.Enabled = false;
            }
            else
            {
                button8.Text = "Run";
                label14.Text = "inactive";
                label14.ForeColor = Color.Red;
                button7.Enabled = true;
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
            }
            if (Properties.Settings.Default.autoStartup)
            {
                button6.Text = "Disable";
            }
            else
            {
                button6.Text = "Enable";
            }
        }

        // Tool strip menu - open
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = true;
            timer1.Start();
        }
        // Tool strip menu - exit
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Start when system startup button
        private void button6_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.autoStartup = !Properties.Settings.Default.autoStartup;
            if (Properties.Settings.Default.autoStartup)
            {
                //Set the application to run at startup
                CreateTask(folderName, taskName, programPath);
            }
            else
            {
                //Delete the application from the startup
                DeleteTask(folderName, taskName);
            }
            Properties.Settings.Default.Save();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = true;
            timer1.Start();
        }
    }
}
