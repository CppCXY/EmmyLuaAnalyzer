using System.Net;
using System.Net.Sockets;
using LanguageServer.CodeAction;
using LanguageServer.Completion;
using LanguageServer.Definition;
using LanguageServer.DocumentColor;
using LanguageServer.DocumentLink;
using LanguageServer.DocumentRender;
using LanguageServer.DocumentSymbol;
using LanguageServer.ExecuteCommand;
using LanguageServer.Hover;
using LanguageServer.InlayHint;
using LanguageServer.InlineValues;
using LanguageServer.References;
using LanguageServer.Rename;
using LanguageServer.SemanticToken;
using LanguageServer.Server;
using LanguageServer.SignatureHelper;
using LanguageServer.TextDocument;
using LanguageServer.TypeHierarchy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        .WithHandler<CodeActionHandler>()
        .WithHandler<ExecuteCommandHandler>()
        .WithHandler<SignatureHelperHandler>()
        .WithHandler<EmmyAnnotatorHandler>()
        .WithHandler<InlineValuesHandler>()
        .WithHandler<DocumentLinkHandler>()
        .WithHandler<TypeHierarchyHandler>()
        .WithServices(services =>
        {
            services.AddSingleton<ServerContext>(
                server => new ServerContext(server.GetRequiredService<ILanguageServerFacade>())
            );
            services.AddLogging(b => b.SetMinimumLevel(LogLevel.Trace));
        })
        .OnInitialize((server, request, token) =>
        {
            workspacePath = request.RootPath;
            return Task.CompletedTask;
        })
        .OnStarted((server, _) =>
        {
            var context = server.GetRequiredService<ServerContext>();
            context.StartServer(workspacePath);
            return Task.CompletedTask;
        });
}).ConfigureAwait(false);
await server.WaitForExit.ConfigureAwait(false);