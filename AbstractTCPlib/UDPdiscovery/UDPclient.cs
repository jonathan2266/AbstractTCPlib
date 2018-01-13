using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace AbstractTCPlib.UDPdiscovery
{
    public class UDPclient : IDisposable
    {
        private string broadcastMessageClient;
        private int destinationBroadCastPort;
        private UdpClient client = null;
        private IPEndPoint end;
        private List<TcpClient> resultedClients = new List<TcpClient>();

        public UDPclient(string broadcastMessage, int destinationBroadCastPort)
        {
            broadcastMessageClient = broadcastMessage + "client";
            this.destinationBroadCastPort = destinationBroadCastPort;

            client = new UdpClient();
            client.EnableBroadcast = true;
        }
        public TcpClient[] Broadcast(int tcpListenPort)
        {
            end = new IPEndPoint(IPAddress.Broadcast, destinationBroadCastPort);

            byte[] encodedMessage = Encoding.ASCII.GetBytes(broadcastMessageClient + tcpListenPort);

            client.Send(encodedMessage, encodedMessage.Length, end);
            
            TcpListener lis = new TcpListener(IPAddress.Any, tcpListenPort);
            lis.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            lis.Start();

            TimeSpan span = new TimeSpan(0, 0, 5);
            DateTime start = DateTime.UtcNow;
            DateTime check;

            while (!lis.Pending())
            {
                Thread.Sleep(2);
                check = DateTime.UtcNow;

                if (check - start > span)
                {
                    break;
                }
            }

            while (lis.Pending())
            {
                resultedClients.Add(lis.AcceptTcpClient());
            }

            lis.Stop();

            return resultedClients.ToArray();
        }

        public void Dispose()
        {
            if (client != null)
            {
                client.Close();
            }
        }
    }
}
