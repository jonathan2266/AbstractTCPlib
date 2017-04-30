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
            client.EnableBroadcast = true;
            end = new IPEndPoint(IPAddress.Broadcast, port);
        }
        public TcpClient Broadcast()
        {
            client.Send(broadcastMessageClient, broadcastMessageClient.Length, end);

            Thread listener = new Thread(new ThreadStart(broadCastThread));
            listener.Start();

            Thread.Sleep(500);

            if (listener.IsAlive)
            {
                listener.Abort();
            }

            end = new IPEndPoint(IPAddress.Broadcast, port);

            return resultedClient;
        }

        private void broadCastThread()
        {
            byte[] res = null;

            while (true)
            {
                res = client.Receive(ref end);
                if (res == null)
                {
                    break;
                }
                if (broadcastMessageMaster.SequenceEqual(res))
                {
                    try
                    {
                        TcpListener lis = new TcpListener(IPAddress.Any, port);
                        lis.Start();
                        resultedClient = lis.AcceptTcpClient();
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("UDPClient: " + e);
                        
                    }
                }
            }
        }
    }
}
