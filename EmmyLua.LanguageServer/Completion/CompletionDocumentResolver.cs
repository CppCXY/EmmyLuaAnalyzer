using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Markup;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Server.Render;


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
        if (completionItem.Data?.Value is int intId)
        {
            var documentId = new LuaDocumentId(intId);
            if (context.GetSemanticModel(documentId) is {} semanticModel)
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
            if (completionItem.Data?.Value is string strPtr)
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