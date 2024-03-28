using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;


namespace LanguageServer.Hover;

// ReSharper disable once ClassNeverInstantiated.Global
public class HoverHandler(
    LuaWorkspace workspace
) : HoverHandlerBase
{
    protected override HoverRegistrationOptions CreateRegistrationOptions(HoverCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new HoverRegistrationOptions()
        {
            DocumentSelector = ToSelector.ToTextDocumentSelector(workspace)
        };
    }

    public override Task<OmniSharp.Extensions.LanguageServer.Protocol.Models.Hover?> Handle(HoverParams request,
        CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
        var semanticModel = workspace.Compilation.GetSemanticModel(uri);
        if (semanticModel is not null)
        {
            var document = semanticModel.Document;
            var pos = request.Position;
            var node = document.SyntaxTree.SyntaxRoot.NodeAt(pos.Line, pos.Character);
            var hoverResult = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Hover()
            {
                Contents = new MarkedStringsOrMarkupContent(new MarkupContent()
                {
                    Kind = MarkupKind.Markdown,
                    Value = semanticModel.RenderSymbol(node)
                })
            };
            return Task.FromResult(hoverResult)!;
        }

        return Task.FromResult<OmniSharp.Extensions.LanguageServer.Protocol.Models.Hover?>(null);
    }
}