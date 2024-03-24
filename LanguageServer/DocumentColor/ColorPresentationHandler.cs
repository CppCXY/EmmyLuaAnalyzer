using EmmyLua.CodeAnalysis.Workspace;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.DocumentColor;

// ReSharper disable once ClassNeverInstantiated.Global
class ColorPresentationHandler(LuaWorkspace workspace) : ColorPresentationHandlerBase
{
    private DocumentColorBuilder Builder { get; } = new();
    
    public override Task<Container<ColorPresentation>> Handle(ColorPresentationParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
        var semanticModel = workspace.Compilation.GetSemanticModel(uri);
        if (semanticModel is not null)
        {
            var result = Builder.ModifyColor(request, semanticModel);
            return Task.FromResult(new Container<ColorPresentation>(result));
        }

        return Task.FromResult(new Container<ColorPresentation>());
    }
}