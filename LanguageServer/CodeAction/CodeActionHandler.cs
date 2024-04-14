using LanguageServer.Server;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.CodeAction;

// ReSharper disable once ClassNeverInstantiated.Global
public class CodeActionHandler(ServerContext context) : CodeActionHandlerBase
{
    private CodeActionBuilder Builder { get; } = new();
    
    protected override CodeActionRegistrationOptions CreateRegistrationOptions(CodeActionCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new CodeActionRegistrationOptions()
        {
            DocumentSelector = ToSelector.ToTextDocumentSelector(context.LuaWorkspace),
            ResolveProvider = true,
            CodeActionKinds = new Container<CodeActionKind>(CodeActionKind.QuickFix)
        };
    }

    public override Task<CommandOrCodeActionContainer?> Handle(CodeActionParams request,
        CancellationToken cancellationToken)
    {
        var result = new List<CommandOrCodeAction>();
        var uri = request.TextDocument.Uri.ToUnencodedString();
        var diagnostics = request.Context.Diagnostics;
        context.ReadyRead(() =>
        {
            result = Builder.Build(diagnostics, uri, context);
        });
        
        return Task.FromResult<CommandOrCodeActionContainer?>(result);
    }

    public override Task<OmniSharp.Extensions.LanguageServer.Protocol.Models.CodeAction> Handle(
        OmniSharp.Extensions.LanguageServer.Protocol.Models.CodeAction request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request);
    }
}