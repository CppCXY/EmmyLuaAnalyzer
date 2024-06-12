using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.LanguageServer.Server;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace EmmyLua.LanguageServer.Completion;

public class CompletionDocumentResolver
{
    private LuaRenderFeature RenderFeature { get; } = new ()
    {
        ExpandAlias = true,
        ShowTypeLink = false,
        InHint = false,
        MaxStringPreviewLength = 100
    };
    
    public CompletionItem Resolve(CompletionItem completionItem, ServerContext context)
    {
        switch (completionItem.Kind)
        {
            case CompletionItemKind.Module:
            case CompletionItemKind.File:
            case CompletionItemKind.Field:
            {
                return ModuleResolve(completionItem, context);
            }
            default:
            {
                return GeneralResolve(completionItem, context);
            }
        }
    }

    private CompletionItem ModuleResolve(CompletionItem completionItem, ServerContext context)
    {
        if (completionItem.Data is not null && completionItem.Data.Type == JTokenType.String)
        {
            var id = new LuaDocumentId((int) completionItem.Data);
            if (context.GetSemanticModel(id) is {} semanticModel)
            {
                completionItem = completionItem with
                {
                    Documentation = new MarkupContent()
                    {
                        Kind = MarkupKind.Markdown,
                        Value = semanticModel.RenderBuilder.RenderModule(semanticModel.Document, RenderFeature)
                    }
                };
            }
        }

        return completionItem;
    }

    private CompletionItem GeneralResolve(CompletionItem completionItem, ServerContext context)
    {
        if (completionItem.Data is not null)
        {
            if (completionItem.Data.Type == JTokenType.String && (string?) completionItem.Data is { } strPtr)
            {
                var ptr = LuaElementPtr<LuaSyntaxNode>.From(strPtr);
                var node = ptr.ToNode(context.LuaWorkspace);
                if (node is null)
                {
                    return completionItem;
                }
                
                if (context.GetSemanticModel(ptr.DocumentId) is {} semanticModel)
                {
                    return completionItem with
                    {
                        Documentation = new MarkupContent()
                        {
                            Kind = MarkupKind.Markdown,
                            Value = semanticModel.RenderBuilder.Render(node, RenderFeature)
                        }
                    };
                }
            }
        }

        return completionItem;
    }
}