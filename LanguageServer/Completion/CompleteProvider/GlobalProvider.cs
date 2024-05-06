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
            .GetDeclarationsBefore(context.TriggerToken)
            .Select(it => it.Name)
            .ToHashSet();

        var globals = context.SemanticModel.GetGlobals();
        foreach (var globalDecl in globals)
        {
            if (!localHashSet.Contains(globalDecl.Name))
            {
                context.CreateCompletion(globalDecl.Name, globalDecl.Info.DeclarationType)
                    .WithData(globalDecl.Info.Ptr.Stringify)
                    .WithCheckDeclaration(globalDecl)
                    .AddToContext();
            }
        }
    }
}