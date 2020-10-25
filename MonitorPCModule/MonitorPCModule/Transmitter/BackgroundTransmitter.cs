using System.IO;

namespace MonitorPCModule.Transmitter
{
    public class BackgroundTransmitter : TcpTransmitter
    {
        private readonly string _jpgPath;

        public BackgroundTransmitter(string hostname, int port, string jpgPath) : base(hostname, port)
        {
            _jpgPath = jpgPath;
        }

        protected override PackageType GetPackageType()
        {
            return PackageType.StaticBackgroundImage;
        }

        protected override byte[] GetData()
        {
            return File.ReadAllBytes(_jpgPath);
        }
    }
}