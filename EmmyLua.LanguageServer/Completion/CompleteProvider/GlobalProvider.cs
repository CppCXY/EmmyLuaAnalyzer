using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.LanguageServer.Completion.CompleteProvider;

public class GlobalProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        if (context.TriggerToken?.Parent is LuaNameExprSyntax)
        {
            AddGlobalCompletion(context);
        }
        // else if (context.TriggerToken is LuaWhitespaceToken)
        // {
        //     AddGlobalCompletion(context);
        // }
        // else if (context.TriggerToken is { Kind: LuaTokenKind.TkEndOfLine })
        // {
        //     AddGlobalCompletion(context);
        // }
    }

    private void AddGlobalCompletion(CompleteContext context)
    {
        if (context.TriggerToken is not null)
        {
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
                        .WithData(globalDecl.RelationInformation)
                        .WithCheckDeclaration(globalDecl)
                        .AddToContext();
                }
            }
        }
    }
}