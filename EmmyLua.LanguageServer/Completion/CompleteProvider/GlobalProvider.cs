using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.LanguageServer.Completion.CompleteProvider;

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
                context.CreateCompletion(globalDecl.Name, globalDecl.Type)
                    .WithData(globalDecl.RelationInfomation)
                    .WithCheckDeclaration(globalDecl)
                    .AddToContext();
            }
        }
    }
}