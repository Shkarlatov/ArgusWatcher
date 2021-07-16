using System;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO.Ports;

namespace ArgusWatcher
{
    public partial class Form1 : Form
    {
        SerialPort serial = new SerialPort();
        NotifyIcon notify = new NotifyIcon();
        ContextMenuStrip contextmenu = new ContextMenuStrip();
        Timer timer = new Timer();
        int lastFlag = -1;

        int timeout = 1000 * 3;
        string lastMessage = "Пожалуйста, подождите...";
        string lastTitle = "Инициализация";
        ToolTipIcon lastIcon = ToolTipIcon.None;
        TimeSpan timespan;

        public Form1()
        {
            InitializeComponent();
            LoadSettings();
            InitTray();
            Application.ApplicationExit += Application_ApplicationExit;
            
        }

        private void Application_ApplicationExit(object sender, EventArgs e)
        {
            Send("0");
            serial.Close();
            Application.Exit();
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            if(serial.IsOpen)
            {
                Send("0");
                serial.Close();
            }
        }

        void LoadSettings()
        {
            this.Icon = Properties.Resources.logo;
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;

            comboBox_port.Items.AddRange(SerialPort.GetPortNames());
            comboBox_port.SelectedItem = Properties.Settings.Default.Port;

            serial.PortName = comboBox_port.SelectedItem != null ? comboBox_port.SelectedItem.ToString():comboBox_port.Items[0].ToString();
            serial.DataReceived += Serial_DataReceived;
            serial.Open();

            timer.Interval = 1000 * 3;
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            System.Threading.Thread th = new System.Threading.Thread(GetStatus);
            th.Start();
        }

        void GetStatus()
        {
            Send("?");
            System.Threading.Thread.Sleep(200);
            Send("t");
        }


        private void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string res = serial.ReadLine().Trim();
            Parser(res);
        }

        void Parser(string res)
        {
            Console.WriteLine(res);
            if(res.Length==1)
            {
                int current = int.Parse(res);
                if(lastFlag!=current)
                {
                    lastFlag = current;
                    switch (lastFlag)
                    {
                        case 0:
                            lastIcon = ToolTipIcon.Error;
                            lastTitle = "Предупреждение";
                            lastMessage = "Средства защиты выключены.";
                            notify.Icon = Properties.Resources.off;
                            break;
                        case 1:
                            lastIcon = ToolTipIcon.Info;
                            lastTitle = "Информация";
                            lastMessage = "Средства защиты работают в штатном режиме.";
                            notify.Icon = Properties.Resources.on;
                            break;
                        case 3:
                            lastIcon = ToolTipIcon.Warning;
                            lastTitle = "Ошибка!";
                            lastMessage = "Нет излучения от средств защиты.";
                            notify.Icon = Properties.Resources.warning;
                            break;
                        case 4:
                            lastIcon = ToolTipIcon.Info;
                            lastTitle = "Информация";
                            lastMessage = "Средства защиты работают в штатном режиме.";
                            notify.Icon = Properties.Resources.on;
                            break;
                    }
                    notify.ShowBalloonTip(timeout, lastTitle, lastMessage, lastIcon);
                }
                
            }
            if(res.Length>1)
            {
                timespan = TimeSpan.FromMinutes(Convert.ToInt32(res, 16));
            }
        }

        void InitTray()
        {
            contextmenu.Items.Add("Включить", Properties.Resources.on.ToBitmap(), action_on);
            contextmenu.Items.Add("Выключить", Properties.Resources.off.ToBitmap(), action_off);
            contextmenu.Items.Add(new ToolStripSeparator());
            contextmenu.Items.Add("Настройки", null, action_settings);
            contextmenu.Items.Add(new ToolStripSeparator());
            contextmenu.Items.Add("Выход", null, action_exit);

            notify.Icon = Properties.Resources.logo;
            notify.ContextMenuStrip = contextmenu;
            notify.Visible = true;
            notify.MouseClick += Notify_MouseClick;
        }

        private void Notify_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                notify.ShowBalloonTip(timeout, lastTitle, lastMessage, lastIcon);
            }
            if(e.Button==MouseButtons.Middle)
            {
                notify.ShowBalloonTip(timeout, "Время работы", $"{timespan.Days}д. {timespan.Hours}ч. {timespan.Minutes}м.", ToolTipIcon.Info);
            }
        }

        void action_on(object o, EventArgs e)
        {
            Send("1");
        }

        void action_off(object o, EventArgs e)
        {
            Send("0");
        }

        void action_settings(object o, EventArgs e)
        {
            DialogResult res = ShowDialog();
            if(res==DialogResult.OK)
            {
                Properties.Settings.Default.Port = comboBox_port.SelectedItem?.ToString();

                Properties.Settings.Default.Save();
                timer.Stop();
                timer.Interval = 1000 * 3;
                serial.Close();
                serial.PortName = comboBox_port.SelectedItem != null ? comboBox_port.SelectedItem.ToString() : comboBox_port.Items[0].ToString();
                serial.Open();
                timer.Start();
            }
        }

        void action_exit(object o, EventArgs e)
        {
            Application_ApplicationExit(o,e);
        }

        void Send(string msg)
        {
            if (serial.IsOpen)
                serial.Write(msg);
        }
    }
}
