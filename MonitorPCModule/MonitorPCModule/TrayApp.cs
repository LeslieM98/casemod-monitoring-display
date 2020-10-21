using HardwareTemperature;
using Microsoft.Extensions.Configuration;
using MonitorPCModule.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonitorPCModule
{
    class TrayApp : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private NetworkTransmitter network;

        public TrayApp()
        {
            // Initialize Tray Icon
            trayIcon = new NotifyIcon()
            {
                Icon = Icon.FromHandle(Resources.icon.GetHicon()),
                ContextMenu = new ContextMenu(new MenuItem[] { new MenuItem("Exit", Exit), new MenuItem("Debug", Debug) }),
                Visible = true
            };

            IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile("configuration.json").Build();

            network = new NetworkTransmitter(config["device_hostname"], int.Parse(config["port"]), int.Parse(config["update_in_ms"]));
            network.Start();
        }

        void ShowTip(object sender, EventArgs e)
        {
            trayIcon.ShowBalloonTip(3);
        }

        void Exit(object sender, EventArgs e)
        {
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            trayIcon.Visible = false;

            Application.Exit();
        }

        void Debug(object sender, EventArgs e)
        {
            MessageBox.Show("Last package sent: " + network.LastPackage.ToString());
        }
    }
}
