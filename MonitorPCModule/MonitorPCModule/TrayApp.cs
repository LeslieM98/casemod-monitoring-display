using Microsoft.Extensions.Configuration;
using System;
using System.Drawing;
using System.Windows.Forms;
using MonitorPCModule.Transmitter;
using MonitorPCModule.Transmitter.Properties;

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
                ContextMenu = new ContextMenu(new[] {  new MenuItem("Debug", Debug), new MenuItem("Exit", Exit) }),
                Visible = true
            };

            try
            {
                IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile("configuration.json").Build();
                var deviceHostname = config["device_hostname"];
                var port = int.Parse(config["port"]);
                var updateInterval = int.Parse(config["update_in_ms"]);
                
                network = new NetworkTransmitter(deviceHostname, port, updateInterval);
                TcpTransmitter configTransmitter = new ConfigTransmitter(deviceHostname, port, "layout.json");
                if (config["background_image_path"] != null)
                {
                    var backgroundImagePath = config["background_image_path"];
                    new BackgroundTransmitter(deviceHostname, port, backgroundImagePath).Start();
                }


                configTransmitter.Start();
                network.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                trayIcon.Visible = false;
                Environment.Exit(1);  
            }
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
