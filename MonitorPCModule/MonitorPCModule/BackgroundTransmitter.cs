using OpenHardwareMonitor.Hardware;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace MonitorPCModule
{
    public class BackgroundTransmitter
    {
        private string Hostname;
        private int Port;
        private string JpgPath;

        public BackgroundTransmitter(string hostname, int port, string jpg_path)
        {
            Hostname = hostname;
            Port = port;
            JpgPath = jpg_path;
        }

        public void Start()
        {
            byte[] send_buffer = File.ReadAllBytes(JpgPath);
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint endPoint = new IPEndPoint(findIp(Hostname), Port);
            
            sock.SendTo(send_buffer, endPoint);
        }
        
        private IPAddress findIp(String hostname)
        {

            IPHostEntry hostEntry = Dns.GetHostEntry(hostname);

            foreach (IPAddress ip in hostEntry.AddressList)
            {
                if (!ip.ToString().Contains(":"))
                {
                    return ip;
                }
            }
            return null;
        }
    }
}