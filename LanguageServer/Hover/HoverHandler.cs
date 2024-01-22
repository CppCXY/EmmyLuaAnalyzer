using EmmyLua.CodeAnalysis.Workspace;
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
            DocumentSelector = new TextDocumentSelector
            (
                new TextDocumentFilter()
                {
                    Pattern = "**/*.lua"
                }
            )
        };
    }

    public override Task<OmniSharp.Extensions.LanguageServer.Protocol.Models.Hover?> Handle(HoverParams request,
        CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
        var document = workspace.GetDocument(uri);

        if (document is not null)
        {
            var pos = request.Position;
            var declarationTree = workspace.Compilation.GetSymbolTree(document.Id);
            var node = declarationTree?.LuaSyntaxTree.SyntaxRoot.NodeAt(pos.Line, pos.Character);
            if (node is not null)
            {
                var declaration = declarationTree?.FindDeclaration(node);
                if (declaration is not null)
                {
                    var hoverResult = new OmniSharp.Extensions.LanguageServer.Protocol.Models.Hover()
                    {
                        Contents = new MarkedStringsOrMarkupContent(new MarkupContent()
                        {
                            Kind = MarkupKind.Markdown,
                            Value = $"{declaration.Type?.ToDisplayString(workspace.Compilation.SearchContext)}"
                        })
                    };
                    return Task.FromResult(hoverResult)!;
                }
            }
        }

        return Task.FromResult<OmniSharp.Extensions.LanguageServer.Protocol.Models.Hover?>(null);
    }
}