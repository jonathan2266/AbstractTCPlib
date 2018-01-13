# AbstractTCPlib
library with build in UDP discovery and easy tcp data transfer

## TCPgeneral

```csharp
TCPgeneral gen = new TCPgeneral(TcpClient client, int uniqueID);
gen.OnRawDataRecieved += OnRecieved; //event
gen.OnError += OnError; //event
gen.start(); // the listening and recieve threads get started

private void OnRecieved(int id, byte[] data){ } //with multiple TCPgeneral instances in a list you know which one recieved the data by the ID
private void OnError(int id, ErrorTypes type, string message){} //if any internal error happens like a disconnect

gen.SendTCP(byte[] data){}; //max payload is int.MaxValue - 4

bool state = gen.IsAlive; //when false you should destroy this object
```

## UDPmaster

```csharp
UDPmaster master = new UDPmaster(string message, int localUDPport);
TcpClient client = master.Listen(); //this is blocking


UDPclient cl = new UDPclient(string message, int destinationUDPport);
TcpClient[] clients = cl.Broadcast(int tcpPort); //broadcasts and waits for 5 seconds int defines the port used to establish a tcp connection

```

the message string and the port has to be the same for both UDPmaster and UDPClient
