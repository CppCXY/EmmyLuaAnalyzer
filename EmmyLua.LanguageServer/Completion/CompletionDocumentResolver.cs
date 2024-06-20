using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Server.Render;
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
        MaxStringPreviewLength = 100,
        InHover = true
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
                var renderBuilder = new LuaRenderBuilder(semanticModel.Context);
                completionItem = completionItem with
                {
                    Documentation = new MarkupContent()
                    {
                        Kind = MarkupKind.Markdown,
                        Value = renderBuilder.RenderModule(semanticModel.Document, RenderFeature)
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
                    var renderBuilder = new LuaRenderBuilder(semanticModel.Context);
                    return completionItem with
                    {
                        Documentation = new MarkupContent()
                        {
                            Kind = MarkupKind.Markdown,
                            Value = renderBuilder.Render(node, RenderFeature)
                        }
                    };
                }
            }
        }

        return completionItem;
    }
}