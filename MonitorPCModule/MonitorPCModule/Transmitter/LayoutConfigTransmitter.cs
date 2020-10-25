using System;

namespace MonitorPCModule.Transmitter
{
    public class ConfigTransmitter : TcpTransmitter
    {
        private readonly string _configJson;

        public ConfigTransmitter(string hostname, int port, string configJson) : base(hostname, port)
        {
            if (hostname == null) throw new ArgumentNullException(nameof(hostname));
            _configJson = configJson ?? throw new ArgumentNullException(nameof(configJson));
        }

        protected override byte[] GetData()
        {
            return System.Text.Encoding.ASCII.GetBytes(_configJson);
        }

        protected override PackageType GetPackageType()
        {
            return PackageType.LayoutConfig;
        }
    }
}