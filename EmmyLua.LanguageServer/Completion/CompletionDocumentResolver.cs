using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render.Renderer;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Workspace;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace EmmyLua.LanguageServer.Completion;

public class CompletionDocumentResolver
{
    private LuaWorkspace Workspace { get; }
    private SearchContext Context { get; }

    private LuaRenderFeature RenderFeature { get; } = new(
        true,
        false,
        false,
        100
    );

    private LuaRenderBuilder RenderBuilder { get; }

    public CompletionDocumentResolver(LuaWorkspace workspace)
    {
        Workspace = workspace;
        Context = new SearchContext(workspace.Compilation, new SearchContextFeatures());
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
        if (completionItem.Data is not null && completionItem.Data.Type == JTokenType.String)
        {
            var id = new LuaDocumentId((int) completionItem.Data);
            var document = Workspace.GetDocument(id);
            if (document is not null)
            {
                ;
                completionItem = completionItem with
                {
                    Documentation = new MarkupContent()
                    {
                        Kind = MarkupKind.Markdown,
                        Value = RenderBuilder.RenderModule(document, RenderFeature)
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
            if (completionItem.Data.Type == JTokenType.String && (string?) completionItem.Data is { } strPtr)
            {
                var ptr = LuaElementPtr<LuaSyntaxNode>.From(strPtr);
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
                        Value = RenderBuilder.Render(node, RenderFeature)
                    }
                };
            }
        }

        return completionItem;
    }
}