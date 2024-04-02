using System.Net;
using System.Net.Sockets;
using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.Completion;
using LanguageServer.Configuration;
using LanguageServer.Definition;
using LanguageServer.DocumentColor;
using LanguageServer.DocumentSymbol;
using LanguageServer.ExecuteCommand;
using LanguageServer.Hover;
using LanguageServer.InlayHint;
using LanguageServer.References;
using LanguageServer.Rename;
using LanguageServer.SemanticToken;
using LanguageServer.SignatureHelper;
using LanguageServer.TextDocument;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        .WithHandler<DidChangeWatchedFilesHandler>()
        .WithHandler<HoverHandler>()
        .WithHandler<DefinitionHandler>()
        .WithHandler<ReferencesHandler>()
        .WithHandler<RenameHandler>()
        .WithHandler<InlayHintHandler>()
        .WithHandler<DocumentSymbolHandler>()
        .WithHandler<DocumentColorHandler>()
        .WithHandler<ColorPresentationHandler>()
        .WithHandler<SemanticTokenHandler>()
        .WithHandler<CompletionHandler>()
        .WithHandler<ExecuteCommandHandler>()
        .WithHandler<SignatureHelperHandler>()
        .WithServices(services =>
        {
            services.AddSingleton<LuaWorkspace>(_ => LuaWorkspace.Create(""));
            services.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace));
            services.AddSingleton<LuaConfig>(server => new LuaConfig(server.GetRequiredService<ILogger<LuaConfig>>()));
        })
        .OnInitialize((server, request, token) =>
        {
            workspacePath = request.RootPath;
            return Task.CompletedTask;
        })
        .OnInitialized((server, request, response, token) =>
        {
            server.Services.GetService<LuaConfig>()?.Watch(Path.Combine(workspacePath, ".luarc"));
            server.Services.GetService<LuaWorkspace>()?.LoadWorkspace(workspacePath);
            return Task.CompletedTask;
        });
}).ConfigureAwait(false);
await server.WaitForExit.ConfigureAwait(false);
