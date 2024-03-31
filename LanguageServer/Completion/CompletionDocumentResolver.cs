using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Workspace;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion;

public class CompletionDocumentResolver
{
    private LuaWorkspace Workspace { get; }
    private SearchContext Context { get; }

    private LuaRenderBuilder RenderBuilder { get; }

    public CompletionDocumentResolver(LuaWorkspace workspace)
    {
        Workspace = workspace;
        Context = new SearchContext(workspace.Compilation);
        RenderBuilder = new LuaRenderBuilder(Context);
    }

    public CompletionItem Resolve(CompletionItem completionItem)
    {
        switch (completionItem.Kind)
        {
            case CompletionItemKind.Module:
            case CompletionItemKind.File:
            case CompletionItemKind.Field:
            {
                return ModuleResolve(completionItem);
            }
            default:
            {
                return GeneralResolve(completionItem);
            }
        }
    }

    private CompletionItem ModuleResolve(CompletionItem completionItem)
    {
        if (completionItem.Data is not null)
        {
            var id = new LuaDocumentId((int)completionItem.Data);
            var sb = new StringBuilder();
            var document = Workspace.GetDocument(id);
            if (document is not null)
            {
                LuaModuleRender.RenderModule(document, Context, sb);
                completionItem = completionItem with
                {
                    Documentation = new MarkupContent()
                    {
                        Kind = MarkupKind.Markdown,
                        Value = sb.ToString()
                    }
                };
            }
        }

        return completionItem;
    }

    private CompletionItem GeneralResolve(CompletionItem completionItem)
    {
        if (completionItem.Data is not null)
        {
            if (completionItem.Data.Type == JTokenType.String && (string?)completionItem.Data is {} strPtr)
            {
                var parts = strPtr.Split('_');
                if (parts.Length != 4)
                {
                    return completionItem;
                }

                var documentId = new LuaDocumentId(int.Parse(parts[0]));
                var range = new SourceRange(int.Parse(parts[1]), int.Parse(parts[2]));
                var kind = (LuaSyntaxKind)int.Parse(parts[3]);
                var ptr = new LuaSyntaxNodePtr<LuaSyntaxNode>(documentId, range, kind);
                var node = ptr.ToNode(Context);
                if (node is null)
                {
                    return completionItem;
                }

                return completionItem with
                {
                    Documentation = new MarkupContent()
                    {
                        Kind = MarkupKind.Markdown,
                        Value = RenderBuilder.Render(node)
                    }
                };
            }
        }

        return completionItem;
    }
}