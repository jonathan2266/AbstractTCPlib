using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AbstractTCPlib.UDPdiscovery
{
    public class UDPclient
    {
        private byte[] broadcastMessageClient;
        private byte[] broadcastMessageMaster;
        private int port;
        private UdpClient client = null;
        private IPEndPoint end;
        private TcpClient resultedClient;

        public UDPclient(string broadcastMessage, int port)
        {
            broadcastMessageClient = Encoding.ASCII.GetBytes(broadcastMessage + "client");
            broadcastMessageMaster = Encoding.ASCII.GetBytes(broadcastMessage + "master");
            this.port = port;

            client = new UdpClient(port);
            client.Client.ReceiveTimeout = 5000;
            client.Client.SendTimeout = 5000;
            client.EnableBroadcast = true;
            end = new IPEndPoint(IPAddress.Broadcast, port);
        }
        public TcpClient Broadcast()
        {
            client.Send(broadcastMessageClient, broadcastMessageClient.Length, end);

            TcpListener lis = new TcpListener(IPAddress.Any, port);
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

            if (lis.Pending())
            {
                resultedClient = lis.AcceptTcpClient();
            }


            end = new IPEndPoint(IPAddress.Broadcast, port);

            return resultedClient;
        }

    }
}
