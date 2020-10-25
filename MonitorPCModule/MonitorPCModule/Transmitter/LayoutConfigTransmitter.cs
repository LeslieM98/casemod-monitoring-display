using System;
using System.IO;

namespace MonitorPCModule.Transmitter
{
    public class ConfigTransmitter : TcpTransmitter
    {
        private readonly string _jsonPath;

        public ConfigTransmitter(string hostname, int port, string jsonPath) : base(hostname, port)
        {
            if (hostname == null) throw new ArgumentNullException(nameof(hostname));
            _jsonPath = jsonPath ?? throw new ArgumentNullException(nameof(jsonPath));
        }

        protected override byte[] GetData()
        {
            string json = File.ReadAllText(_jsonPath);
            return System.Text.Encoding.ASCII.GetBytes(json);
        }

        protected override PackageType GetPackageType()
        {
            return PackageType.LayoutConfig;
        }
    }
}