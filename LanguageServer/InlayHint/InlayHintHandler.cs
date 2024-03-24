using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.ExtensionUtil;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using InlayHintType = OmniSharp.Extensions.LanguageServer.Protocol.Models.InlayHint;

namespace LanguageServer.InlayHint;

// ReSharper disable once ClassNeverInstantiated.Global
public class InlayHintHandler(LuaWorkspace workspace) : InlayHintsHandlerBase
{
    private InlayHintBuilder Builder { get; } = new();
    
    protected override InlayHintRegistrationOptions CreateRegistrationOptions(InlayHintClientCapabilities capability,
        ClientCapabilities clientCapabilities)
    {
        return new InlayHintRegistrationOptions()
        {
            ResolveProvider = true,
            DocumentSelector = new TextDocumentSelector
            (
                new TextDocumentFilter()
                {
                    Pattern = "**/*.lua"
                }
            ),
        };
    }

    public override Task<InlayHintContainer?> Handle(InlayHintParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
        var semanticModel = workspace.Compilation.GetSemanticModel(uri);
        if (semanticModel is not null)
        {
            var range = request.Range.ToSourceRange(semanticModel.Document);
            var hints = Builder.Build(semanticModel, range, cancellationToken);
            return Task.FromResult<InlayHintContainer?>(InlayHintContainer.From(hints));
        }

        return Task.FromResult<InlayHintContainer?>(null);
    }

    public override Task<InlayHintType> Handle(InlayHintType request, CancellationToken cancellationToken)
    {
        // throw new NotImplementedException();
        return Task.FromResult(request);
    }
}