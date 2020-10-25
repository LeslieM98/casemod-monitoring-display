using System;
using System.Net.Sockets;

namespace MonitorPCModule.Transmitter
{
    public abstract class TcpTransmitter
    {
        private string Hostname { get;}
        private int Port { get; }

        protected TcpTransmitter(string hostname, int port)
        {
            Hostname = hostname;
            Port = port;
        }

        private byte[] BuildPackage()
        {
            byte prefix = (byte) GetPackageType();
            byte[] data = GetData();

            byte[] package = new byte[data.Length + 1];
            Array.Copy(data, 0, package, 1, data.Length);
            package[0] = prefix;

            return package;
        }

        public void Start()
        {
            byte[] sendBuffer = BuildPackage();
            TcpClient client = new TcpClient(Hostname, Port);
            NetworkStream stream = client.GetStream();
            stream.Write(sendBuffer, 0, sendBuffer.Length);
            stream.Close();
            client.Close();
        }

        protected abstract PackageType GetPackageType();

        protected abstract byte[] GetData();

        protected enum PackageType
        {
            LayoutConfig = 0,
            StaticBackgroundImage = 1,
            GifBackgroundImage = 2
        }
    }
}