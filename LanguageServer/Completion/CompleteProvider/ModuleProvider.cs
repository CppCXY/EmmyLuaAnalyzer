using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion.CompleteProvider;

public class ModuleProvider: ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        if (!IsMatch(context))
        {
            return;
        }
        
        var semanticModel = context.SemanticModel;
        var globals = context.SemanticModel.GetGlobals();
        foreach (var globalDecl in globals)
        {
            context.Add(new CompletionItem
            {
                Label = globalDecl.Name,
                Kind = CompletionItemKind.Variable,
                Detail = LuaTypeRender.RenderType(globalDecl.DeclarationType, semanticModel.Context)
            });
        }
    }

    private bool IsMatch(CompleteContext context)
    {
        var token = context.TriggerToken;
        return token.Parent is LuaNameExprSyntax;
    }
}