using System.Net;
using System.Net.Sockets;
using EmmyLua.LanguageServer.Framework.Server;
using EmmyLua.LanguageServer.Framework.Server.Handler;

Stream? input = null;
Stream? output = null;

if (args.Length > 0)
{
    var port = int.Parse(args[0]);
    var tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    var ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
    EndPoint endPoint = new IPEndPoint(ipAddress, port);
    tcpServer.Bind(endPoint);
    tcpServer.Listen(1);

    var languageClientSocket = tcpServer.Accept();
    var networkStream = new NetworkStream(languageClientSocket);
    input = networkStream;
    output = networkStream;
}
else
{
    input = Console.OpenStandardInput();
    output = Console.OpenStandardOutput();
}

var ls = LanguageServer.From(input, output)
    .WithHandler<InitializeHandlerBase>();

await ls.Run();