using System;
using System.Net.Sockets;

namespace MonitorPCModule
{
    public class ConfigTransmitter
    {
        private string Hostname;
        private int Port;
        private string configJSON;

        public ConfigTransmitter(string hostname, int port, string config_json)
        {
            Hostname = hostname;
            Port = port;
            configJSON = config_json;
        }
        
        public void Start()
        {
            byte[] encoded_json = System.Text.ASCIIEncoding.ASCII.GetBytes(configJSON);
            byte[] send_buffer = new byte[encoded_json.Length + 1];
            send_buffer[0] = 0x00; // 0x00 = layoutconfig json
            Array.Copy(encoded_json, 0, send_buffer, 1, encoded_json.Length);
            TcpClient client = new TcpClient(Hostname, Port);
            NetworkStream stream = client.GetStream();
            stream.Write(send_buffer, 0, send_buffer.Length);
            stream.Close();
            client.Close(); 
        }
    }
}