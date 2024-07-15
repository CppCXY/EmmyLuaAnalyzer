using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CodeAction;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;


namespace EmmyLua.LanguageServer.CodeAction;

// ReSharper disable once ClassNeverInstantiated.Global
public class CodeActionHandler(ServerContext context) : CodeActionHandlerBase
{
    private CodeActionBuilder Builder { get; } = new();
    
    protected override Task<CodeActionResponse> Handle(CodeActionParams request, CancellationToken token)
    {
        var result = new List<CommandOrCodeAction>();
        var uri = request.TextDocument.Uri.UnescapeUri;
        var diagnostics = request.Context.Diagnostics;
        context.ReadyRead(() =>
        {
            result = Builder.Build(diagnostics, uri, context);
        });
        
        return Task.FromResult<CodeActionResponse?>(new CodeActionResponse(result))!;
    }

    protected override Task<Framework.Protocol.Message.CodeAction.CodeAction> Resolve(Framework.Protocol.Message.CodeAction.CodeAction request, CancellationToken token)
    {
        return Task.FromResult(request);
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.CodeActionProvider = new CodeActionOptions()
        {
            CodeActionKinds = [CodeActionKind.QuickFix],
        };
    }
}