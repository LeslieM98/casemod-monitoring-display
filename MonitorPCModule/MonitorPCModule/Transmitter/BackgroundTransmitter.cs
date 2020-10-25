using OpenHardwareMonitor.Hardware;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace MonitorPCModule.Transmitter
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
            byte[] read_file = File.ReadAllBytes(JpgPath);
            byte[] send_buffer = new byte[read_file.Length + 1];
            send_buffer[0] = 0x01; // 0x01 = static image
            Array.Copy(read_file, 0, send_buffer, 1, read_file.Length);
            TcpClient client = new TcpClient(Hostname, Port);
            NetworkStream stream = client.GetStream();
            stream.Write(send_buffer, 0, send_buffer.Length);
            stream.Close();
            client.Close();
        }
        

    }
}