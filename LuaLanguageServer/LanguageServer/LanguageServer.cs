using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;
using Serilog.Events;
using static OmniSharp.Extensions.LanguageServer.Server.LanguageServer;

namespace LuaLanguageServer.LanguageServer;

public class LanguageServer
{
    public LanguageServer()
    {
    }

    public async Task StartAsync(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
            .MinimumLevel.Verbose()
            .CreateLogger();

        var server = await From(options =>
        {
            IObserver<WorkDoneProgressReport> workDone = null!;
            if (args.Length == 0)
            {
                options.WithOutput(Console.OpenStandardOutput()).WithInput(Console.OpenStandardInput());
            }
            else
            {
                var port = Int32.Parse(args[0]);
                var tcpServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
                EndPoint endPoint = new IPEndPoint(ipAddress, port);
                tcpServer.Bind(endPoint);
                tcpServer.Listen(1);

                var languageClientSocket = tcpServer.Accept();

                var networkStream = new NetworkStream(languageClientSocket);
                options.WithOutput(networkStream).WithInput(networkStream);
            }

            options
                .ConfigureLogging(
                    x => x
                        .AddLanguageProtocolLogging()
                        .SetMinimumLevel(LogLevel.Debug)
                )
                .WithServices(x => x.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace)))
                .OnInitialize((server, request, token) =>
                    {
                        var manager = server.WorkDoneManager.For(
                            request, new WorkDoneProgressBegin
                            {
                                Title = "EmmyLua Server is starting...",
                                Percentage = 10,
                            }
                        );
                        workDone = manager;

                        manager.OnNext(
                            new WorkDoneProgressReport
                            {
                                Percentage = 20,
                                Message = "loading in process"
                            }
                        );
                        return Task.CompletedTask;
                    }
                ).OnInitialized((server, request, response, token) =>
                    {
                        Log.Logger.Debug("workspace completed...");
                        workDone.OnNext(
                            new WorkDoneProgressReport
                            {
                                Message = "loading done",
                                Percentage = 100,
                            }
                        );

                        workDone.OnCompleted();
                        return Task.CompletedTask;
                    }
                );
        }).ConfigureAwait(false);
        await server.WaitForExit.ConfigureAwait(false);
    }
}
