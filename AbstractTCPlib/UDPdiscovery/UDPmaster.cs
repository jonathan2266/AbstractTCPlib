using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AbstractTCPlib.UDPdiscovery
{
    public class UDPmaster : IDisposable
    {
        private byte[] broadcastMessageMaster;
        private byte[] broadcastMessageClient;
        private UdpClient server;
        private IPEndPoint end;
        private int port;

        public UDPmaster(string broadcastMessage, int port)
        {
            broadcastMessageMaster = Encoding.ASCII.GetBytes(broadcastMessage + "master");
            broadcastMessageClient = Encoding.ASCII.GetBytes(broadcastMessage + "client");
            this.port = port;

            server = new UdpClient(port);
            end = new IPEndPoint(IPAddress.Any, port);
        }

        public TcpClient Listen()
        {
            byte[] rec;
            rec = server.Receive(ref end);
            if (broadcastMessageClient.SequenceEqual(rec))
            {
                try
                {
                    TcpClient client = new TcpClient();
                    client.Connect(end);
                    return client;
                }
                catch (Exception e)
                {
                    Console.WriteLine("UDPMaster: " + e);
                    return null;
                }
            }
            return null;
        }

        public void Dispose()
        {
            if (server != null)
            {
                server.Close();
            }
        }
    }
}
