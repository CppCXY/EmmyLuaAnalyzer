using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Server.Render;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;


namespace EmmyLua.LanguageServer.Hover;

// ReSharper disable once ClassNeverInstantiated.Global
public class HoverHandler(
    ServerContext context
) : HoverHandlerBase
{
    private LuaRenderFeature RenderFeature { get; } = new(
        false,
        true,
        false,
        100,
        true
    );
    
    protected override HoverRegistrationOptions CreateRegistrationOptions(HoverCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new HoverRegistrationOptions();
    }

    public override Task<OmniSharp.Extensions.LanguageServer.Protocol.Models.Hover?> Handle(HoverParams request,
        CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUri().AbsoluteUri;
        OmniSharp.Extensions.LanguageServer.Protocol.Models.Hover? hover = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var renderBuilder = new LuaRenderBuilder(semanticModel.Context);
                var document = semanticModel.Document;
                var pos = request.Position;
                var node = document.SyntaxTree.SyntaxRoot.NodeAt(pos.Line, pos.Character);
                hover = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Hover()
                {
                    Contents = new MarkedStringsOrMarkupContent(new MarkupContent()
                    {
                        Kind = MarkupKind.Markdown,
                        Value = renderBuilder.Render(node, RenderFeature)
                    })
                };
            }
        });

        return Task.FromResult(hover);
    }
}