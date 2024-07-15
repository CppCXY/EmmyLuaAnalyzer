using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.InlayHint;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Util;

namespace EmmyLua.LanguageServer.InlayHint;

// ReSharper disable once ClassNeverInstantiated.Global
public class InlayHintHandler(ServerContext context) : InlayHintHandlerBase
{
    private InlayHintBuilder Builder { get; } = new();
    
    protected override Task<InlayHintResponse?> Handle(InlayHintParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.UnescapeUri;
        InlayHintResponse? inlayHintContainer = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var range = request.Range.ToSourceRange(semanticModel.Document);
                var config = context.SettingManager.GetInlayHintConfig();
                var hints = Builder.Build(semanticModel, range, config, cancellationToken);
                inlayHintContainer = new InlayHintResponse(hints);
            }
        });
        
        return Task.FromResult(inlayHintContainer);
    }

    protected override Task<Framework.Protocol.Message.InlayHint.InlayHint> Resolve(Framework.Protocol.Message.InlayHint.InlayHint request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request);
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.InlayHintProvider = new InlayHintsOptions()
        {
            ResolveProvider = false
        };
    }
}