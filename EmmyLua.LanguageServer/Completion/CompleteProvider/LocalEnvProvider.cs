using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.LanguageServer.Completion.CompleteProvider;

public class LocalEnvProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        if (context.TriggerToken?.Parent is not LuaNameExprSyntax)
        {
            return;
        }

        var varDeclarations = context.SemanticModel.GetDeclarationsBefore(context.TriggerToken);
        foreach (var varDeclaration in varDeclarations)
        {
            context.CreateCompletion(varDeclaration.Name, varDeclaration.Type)
                .WithData(varDeclaration.Info.Ptr.Stringify)
                .WithCheckDeclaration(varDeclaration)
                .AddToContext();
        }
    }
}