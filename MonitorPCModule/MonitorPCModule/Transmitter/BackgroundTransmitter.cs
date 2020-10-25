using System.IO;

namespace MonitorPCModule.Transmitter
{
    public class BackgroundTransmitter : TcpTransmitter
    {
        private readonly string _backgroundPath;

        public BackgroundTransmitter(string hostname, int port, string backgroundPath) : base(hostname, port)
        {
            _backgroundPath = backgroundPath.Trim();
        }

        protected override PackageType GetPackageType()
        {
            return _backgroundPath.EndsWith(".gif") ? PackageType.GifBackgroundImage : PackageType.StaticBackgroundImage;
        }

        protected override byte[] GetData()
        {
            return File.ReadAllBytes(_backgroundPath);
        }
    }
}