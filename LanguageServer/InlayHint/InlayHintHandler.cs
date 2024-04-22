using LanguageServer.Configuration;
using LanguageServer.Server;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using InlayHintType = OmniSharp.Extensions.LanguageServer.Protocol.Models.InlayHint;

namespace LanguageServer.InlayHint;

// ReSharper disable once ClassNeverInstantiated.Global
public class InlayHintHandler(ServerContext context) : InlayHintsHandlerBase
{
    private InlayHintBuilder Builder { get; } = new();

    protected override InlayHintRegistrationOptions CreateRegistrationOptions(InlayHintClientCapabilities capability,
        ClientCapabilities clientCapabilities)
    {
        return new InlayHintRegistrationOptions()
        {
            ResolveProvider = true,
            DocumentSelector = ToSelector.ToTextDocumentSelector(context.LuaWorkspace)
        };
    }

    public override Task<InlayHintContainer?> Handle(InlayHintParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
        InlayHintContainer? inlayHintContainer = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var range = request.Range.ToSourceRange(semanticModel.Document);
                var config = context.SettingManager.GetInlayHintConfig();
                var hints = Builder.Build(semanticModel, range, config, cancellationToken);
                inlayHintContainer = InlayHintContainer.From(hints);
            }
        });

        return Task.FromResult<InlayHintContainer?>(inlayHintContainer);
    }

    public override Task<InlayHintType> Handle(InlayHintType request, CancellationToken cancellationToken)
    {
        // throw new NotImplementedException();
        return Task.FromResult(request);
    }
}