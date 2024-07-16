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
using EmmyLua.LanguageServer.Framework.Protocol.Message.Initialize;
using EmmyLua.LanguageServer.Framework.Server;
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

InitializeParams initializeParams = null!;
var ls = LanguageServer.From(input, output);
ls.SupportMultiThread();
ls.AddJsonSerializeContext(EmmyLuaJsonGenerateContext.Default);
var serverContext = new ServerContext(ls);
ls.OnInitialize((c, s) =>
{
    s.Name = "EmmyLua.LanguageServer";
    s.Version = "1.0.0";
    initializeParams = c;
    return Task.CompletedTask;
});
ls.OnInitialized((c) =>
{
    serverContext.StartServerAsync(initializeParams).Wait();
    return Task.CompletedTask;
});
ls.AddHandler(new TextDocumentHandler(serverContext));
ls.AddHandler(new DefinitionHandler(serverContext));
ls.AddHandler(new CompletionHandler(serverContext));
ls.AddHandler(new HoverHandler(serverContext));
ls.AddHandler(new DocumentSymbolHandler(serverContext));
ls.AddHandler(new CodeActionHandler(serverContext));
ls.AddHandler(new CodeLensHandler(serverContext));
ls.AddHandler(new DocumentLinkHandler(serverContext));
ls.AddHandler(new DocumentColorHandler(serverContext));
ls.AddHandler(new RenameHandler(serverContext));
ls.AddHandler(new ExecuteCommandHandler(serverContext));
ls.AddHandler(new DidChangeWatchedFilesHandler(serverContext));
ls.AddHandler(new WorkspaceSymbolHandler(serverContext));
ls.AddHandler(new EmmyAnnotatorHandler(serverContext));
ls.AddHandler(new InlayHintHandler(serverContext));
ls.AddHandler(new InlineValuesHandler(serverContext));
ls.AddHandler(new ReferencesHandler(serverContext));
ls.AddHandler(new SignatureHelperHandler(serverContext));
ls.AddHandler(new SemanticTokenHandler(serverContext));
ls.AddHandler(new TypeHierarchyHandler(serverContext));

await ls.Run();