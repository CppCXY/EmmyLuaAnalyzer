using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SignatureHelp;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Util;

namespace EmmyLua.LanguageServer.SignatureHelper;

// ReSharper disable once ClassNeverInstantiated.Global
public class SignatureHelperHandler(ServerContext context) : SignatureHelpHandlerBase
{
    private SignatureHelperBuilder Builder { get; } = new();

    protected override Task<SignatureHelp> Handle(SignatureHelpParams request, CancellationToken token)
    {
        var uri = request.TextDocument.Uri.Uri.AbsoluteUri;
        SignatureHelp? signatureHelp = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is null)
            {
                return;
            }
        
            var position = request.Position;
            var triggerToken =
                semanticModel.Document.SyntaxTree.SyntaxRoot.TokenLeftBiasedAt(position.Line, position.Character);
            if (triggerToken is not null)
            {
                var config = context.SettingManager.GetSignatureConfig();
                signatureHelp = Builder.Build(semanticModel, triggerToken, request, config);
            }
        });
        
        return Task.FromResult(signatureHelp)!;
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
        serverCapabilities.SignatureHelpProvider = new SignatureHelpOptions()
        {
            TriggerCharacters = ["(", ","],
        };
    }
}