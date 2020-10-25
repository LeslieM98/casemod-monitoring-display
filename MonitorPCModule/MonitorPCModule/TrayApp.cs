using HardwareTemperature;
using Microsoft.Extensions.Configuration;
using MonitorPCModule.Properties;
using System;
using System.Drawing;
using System.IO;
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

            try
            {
                IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile("configuration.json").Build();
                string layout_json = File.ReadAllText("layout.json");
                var device_hostname = config["device_hostname"];
                var port = int.Parse(config["port"]);
                var update_interval = int.Parse(config["update_in_ms"]);
                
                if (config["background_image_path"] != null)
                {
                    var backgroundImagePath = config["background_image_path"];
                    new BackgroundTransmitter(device_hostname, port, backgroundImagePath).Start();
                }


                ConfigTransmitter configTransmitter = new ConfigTransmitter(device_hostname, port, layout_json);
                configTransmitter.Start();
                network = new NetworkTransmitter(device_hostname, port, update_interval);
                
                network.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                trayIcon.Visible = false;
                System.Environment.Exit(1);  
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
