using System.Net;
using System.Net.Sockets;
using EmmyLua.LanguageServer.Framework.Server;


var port = int.Parse(args[0]);
var tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
var ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
EndPoint endPoint = new IPEndPoint(ipAddress, port);
tcpServer.Bind(endPoint);
tcpServer.Listen(1);

var languageClientSocket = tcpServer.Accept();
var networkStream = new NetworkStream(languageClientSocket);
var ls = LanguageServer.From(networkStream, networkStream);
await ls.Run();