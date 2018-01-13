using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AbstractTCPlib;
using AbstractTCPlib.UDPdiscovery;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AbstractTcplibTests
{
    [TestClass]
    public class UDPdiscoveryTests
    {
        const string secret = "lel";
        const string message = "message";

        static AutoResetEvent disposeBlock = new AutoResetEvent(false);
        static AutoResetEvent startBlock = new AutoResetEvent(false);
        static AutoResetEvent mainBlock = new AutoResetEvent(false);

        [TestMethod]
        public void Broadcast()
        {
            string recievedMessage = "not right";

            Task listener = new Task(UdpMaster);
            listener.Start();
            
            UDPclient client = new UDPclient(secret, 5000);
            TcpClient[] clients = new TcpClient[0];

            while (clients.Length == 0)
            {
                clients = client.Broadcast(6001);
            }

            TCPgeneral general = new TCPgeneral(clients.First(), 0);
            general.OnRawDataRecieved += (id, data) =>
            {
                recievedMessage = Encoding.ASCII.GetString(data);

                disposeBlock.Set();
                mainBlock.Set();
            };

            general.Start();

            startBlock.Set();

            mainBlock.WaitOne();

            Assert.IsTrue(recievedMessage.Equals(message));
        }

        private static void UdpMaster()
        {
            UDPmaster master = new UDPmaster(secret, 5000);
            TcpClient client = master.Listen();

            TCPgeneral tcp = new TCPgeneral(client, 100);

            tcp.Start();

            startBlock.WaitOne();

            tcp.SendTCP(Encoding.ASCII.GetBytes(message));

            disposeBlock.WaitOne();

            tcp.Dispose();
        }
    }
}
