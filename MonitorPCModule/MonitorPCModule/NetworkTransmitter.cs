using OpenHardwareMonitor.Hardware;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace HardwareTemperature
{
    class NetworkTransmitter
    {
        private string Hostname;
        private int Port;
        private int Interval;
        public DateTime LastPackage { get; set; }

        public NetworkTransmitter(string hostname, int port, int interval_in_ms)
        {
            Hostname = hostname;
            Port = port;
            Interval = interval_in_ms;
        }

        public void Start()
        {
            Computer computer = new Computer() { CPUEnabled = true, GPUEnabled = true };
            computer.Open();
            GPU gpu = new GPU(computer);
            CPU cpu = new CPU(computer);

            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint endPoint = new IPEndPoint(findIp(Hostname), Port);

            Timer timer = new Timer() { Enabled = true, Interval = Interval };
            timer.Elapsed += delegate (object sender, ElapsedEventArgs e)
            {

                byte[] send_buffer = buildPackage(cpu.Temperature(), gpu.Temperature());
                sock.SendTo(send_buffer, endPoint);
                LastPackage = DateTime.Now;
            };
        }

        private byte[] buildPackage(float cpuTemp, float gpuTemp)
        {
            byte[] cpuPackage = BitConverter.GetBytes(cpuTemp);
            byte[] gpuPackage = BitConverter.GetBytes(gpuTemp);
            byte[] package = new byte[8];
            for (int i = 0; i < 4; i++)
            {
                package[i] = cpuPackage[i];
                package[i + 4] = gpuPackage[i];
            }
            return package;
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
