using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LanguageServer.Completion.CompleteProvider;

public class GlobalProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        if (context.TriggerToken?.Parent is not LuaNameExprSyntax)
        {
            return;
        }

        var localHashSet = context.SemanticModel
            .GetLocalDeclarations(context.TriggerToken)
            .Select(it => it.Name)
            .ToHashSet();
        
        var globals = context.SemanticModel.GetGlobals();
        foreach (var globalDecl in globals)
        {
            if (!localHashSet.Contains(globalDecl.Name))
            {
                context.AddRange(
                    CompletionItemBuilder.Create(globalDecl.Name, globalDecl.DeclarationType, context.SemanticModel)
                        .WithData(globalDecl.Ptr.Stringify)
                        .Build()
                );
            }
        }
    }
}