using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentColor;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.DocumentColor;

// ReSharper disable once ClassNeverInstantiated.Global
public class DocumentColorHandler(ServerContext context) : DocumentColorHandlerBase
{
    private DocumentColorBuilder Builder { get; } = new();
    
    protected override Task<DocumentColorResponse> Handle(DocumentColorParams request, CancellationToken token)
    {
        var uri = request.TextDocument.Uri.Uri.AbsoluteUri;
        DocumentColorResponse? container = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                container = new DocumentColorResponse(Builder.Build(semanticModel));
            }
        });
        
        return Task.FromResult(container)!;
    }

    protected override Task<ColorPresentationResponse> Resolve(ColorPresentationParams request, CancellationToken token)
    {
        var uri = request.TextDocument.Uri.Uri.AbsoluteUri;
        ColorPresentationResponse container = null!;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                container = new ColorPresentationResponse(Builder.ModifyColor(request, semanticModel));
            }
        });
        
        return Task.FromResult(container);
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.ColorProvider = true;
    }
}