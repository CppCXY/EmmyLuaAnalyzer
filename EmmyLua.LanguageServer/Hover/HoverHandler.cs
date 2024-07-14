using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Hover;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Markup;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Server.Render;


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

    protected override Task<HoverResponse> Handle(HoverParams request, CancellationToken token)
    {
        var uri = request.TextDocument.Uri.Uri.AbsoluteUri;
        HoverResponse? hover = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var renderBuilder = new LuaRenderBuilder(semanticModel.Context);
                var document = semanticModel.Document;
                var pos = request.Position;
                var node = document.SyntaxTree.SyntaxRoot.NodeAt(pos.Line, pos.Character);
                hover = new HoverResponse()
                {
                    Contents = new MarkupContent()
                    {
                        Kind = MarkupKind.Markdown,
                        Value = renderBuilder.Render(node, RenderFeature)
                    }
                };
            }
        });

        return Task.FromResult(hover)!;
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
        serverCapabilities.HoverProvider = true;
    }
}