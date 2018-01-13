using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AbstractTCPlib.UDPdiscovery
{
    public class UDPmaster : IDisposable
    {
        private string broadcastMessageClient;
        private UdpClient server;
        private IPEndPoint end;
        private int localUDPClientPort;

        public UDPmaster(string broadcastMessage, int localUDPClientPort)
        {
            broadcastMessageClient = broadcastMessage + "client";
            this.localUDPClientPort = localUDPClientPort;

            server = new UdpClient(this.localUDPClientPort);
            end = new IPEndPoint(IPAddress.Any, 0);
        }

        public TcpClient Listen()
        {
            string recievedMessage = Encoding.ASCII.GetString(server.Receive(ref end));

            if (string.Equals(broadcastMessageClient, recievedMessage.Substring(0, broadcastMessageClient.Length)) && 
                int.TryParse(recievedMessage.Substring(broadcastMessageClient.Length), out int port))
            {
                try
                {
                    end.Port = port;

                    TcpClient client = new TcpClient();
                    client.Connect(end);
                    return client;
                }
                catch (Exception e)
                {
                    Console.WriteLine("UDPMaster: " + e);
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
