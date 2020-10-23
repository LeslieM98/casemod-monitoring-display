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
            TcpClient client = new TcpClient(Hostname, Port);
            NetworkStream stream = client.GetStream();
            stream.Write(send_buffer, 0, send_buffer.Length);
            stream.Close();
            client.Close();
        }
        

    }
}