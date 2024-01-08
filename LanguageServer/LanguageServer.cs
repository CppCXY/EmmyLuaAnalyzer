using System.Net;
using System.Net.Sockets;
using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.TextDocument;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;
using Serilog.Events;
using static OmniSharp.Extensions.LanguageServer.Server.LanguageServer;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
    .MinimumLevel.Verbose()
    .CreateLogger();

var server = await From(options =>
{
    if (args.Length == 0)
    {
        options.WithOutput(Console.OpenStandardOutput()).WithInput(Console.OpenStandardInput());
    }
    else
    {
        var port = int.Parse(args[0]);
        var tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        var ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        EndPoint endPoint = new IPEndPoint(ipAddress, port);
        tcpServer.Bind(endPoint);
        tcpServer.Listen(1);

        var languageClientSocket = tcpServer.Accept();

        var networkStream = new NetworkStream(languageClientSocket);
        options.WithOutput(networkStream).WithInput(networkStream);
    }

    var workspacePath = "";
    options
        .ConfigureLogging(
            x => x
                .AddLanguageProtocolLogging()
                .SetMinimumLevel(LogLevel.Debug)
        )
        .WithHandler<TextDocumentHandler>()
        .WithServices(services =>
        {
            services.AddSingleton<LuaWorkspace>(_ => LuaWorkspace.Create(""));
            services.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace));
        })
        .OnInitialize((server, request, token) =>
        {
            workspacePath = request.RootPath;
            return Task.CompletedTask;
        })
        .OnInitialized((server, request, response, token) => Task.CompletedTask)
        .OnStarted(
            async (languageServer, token) =>
            {
                using var manager = await languageServer.WorkDoneManager
                    .Create(new WorkDoneProgressBegin { Title = "EmmyLua LS Analyzing ..." })
                    .ConfigureAwait(false);

                manager.OnNext(new WorkDoneProgressReport { Message = "doing things..." });
                languageServer.Services.GetService<LuaWorkspace>()?.LoadWorkspace(workspacePath);
                manager.OnCompleted();
            });
}).ConfigureAwait(false);
await server.WaitForExit.ConfigureAwait(false);
