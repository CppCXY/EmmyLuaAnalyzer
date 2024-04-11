using LanguageServer.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.DocumentColor;

// ReSharper disable once ClassNeverInstantiated.Global
class ColorPresentationHandler(ServerContext context) : ColorPresentationHandlerBase
{
    private DocumentColorBuilder Builder { get; } = new();
    
    public override Task<Container<ColorPresentation>> Handle(ColorPresentationParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
        Container<ColorPresentation> container = new Container<ColorPresentation>();
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                container = Builder.ModifyColor(request, semanticModel);
            }
        });
        
        return Task.FromResult(container);
    }
}