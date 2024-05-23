using System.Net;
using System.Net.Sockets;
using EmmyLua.LanguageServer.CodeAction;
using EmmyLua.LanguageServer.CodeLens;
using EmmyLua.LanguageServer.Completion;
using EmmyLua.LanguageServer.Definition;
using EmmyLua.LanguageServer.DocumentColor;
using EmmyLua.LanguageServer.DocumentLink;
using EmmyLua.LanguageServer.DocumentRender;
using EmmyLua.LanguageServer.DocumentSymbol;
using EmmyLua.LanguageServer.ExecuteCommand;
using EmmyLua.LanguageServer.Hover;
using EmmyLua.LanguageServer.InlayHint;
using EmmyLua.LanguageServer.InlineValues;
using EmmyLua.LanguageServer.References;
using EmmyLua.LanguageServer.Rename;
using EmmyLua.LanguageServer.SemanticToken;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.SignatureHelper;
using EmmyLua.LanguageServer.TextDocument;
using EmmyLua.LanguageServer.TypeHierarchy;
using EmmyLua.LanguageServer.WorkspaceSymbol;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
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

    InitializeParams requestParams = null!;
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
        .WithHandler<CodeActionHandler>()
        .WithHandler<ExecuteCommandHandler>()
        .WithHandler<SignatureHelperHandler>()
        .WithHandler<EmmyAnnotatorHandler>()
        .WithHandler<InlineValuesHandler>()
        .WithHandler<DocumentLinkHandler>()
        .WithHandler<TypeHierarchyHandler>()
        .WithHandler<WorkspaceSymbolHandler>()
        .WithHandler<CodeLensHandler>()
        .WithServices(services =>
        {
            services.AddSingleton<ServerContext>(
                server => new ServerContext(
                    server.GetRequiredService<ILanguageServerFacade>()
                )
            );
            services.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace));
        })
        .OnInitialize((server, request, token) =>
        {
            requestParams = request;
            return Task.CompletedTask;
        })
        .OnStarted((server, _) =>
        {
            var context = server.GetRequiredService<ServerContext>();
            context.StartServer(requestParams);
            return Task.CompletedTask;
        });
}).ConfigureAwait(false);
await server.WaitForExit.ConfigureAwait(false);