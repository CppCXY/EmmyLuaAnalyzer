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

        var globals = context.SemanticModel.GetGlobals();
        foreach (var globalDecl in globals)
        {
            context.AddRange(
                CompletionItemBuilder.Create(globalDecl.Name, globalDecl.DeclarationType, context.SemanticModel)
                    .WithData(globalDecl.Ptr.Stringify)
                    .Build()
            );
        }
    }
}