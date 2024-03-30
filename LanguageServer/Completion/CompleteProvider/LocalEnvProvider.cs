using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LanguageServer.Completion.CompleteProvider;

public class LocalEnvProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        if (context.TriggerToken?.Parent is not LuaNameExprSyntax)
        {
            return;
        }

        var varDeclarations = context.SemanticModel.GetDeclarations(context.TriggerToken);
        foreach (var varDeclaration in varDeclarations)
        {
            if (varDeclaration.Feature == DeclarationFeature.Local)
            {
                context.AddRange(
                    CompletionItemBuilder
                        .Create(varDeclaration.Name, varDeclaration.DeclarationType, context.SemanticModel)
                        .WithData(varDeclaration.Ptr.Stringify)
                        .Build()
                );
            }
        }
    }
}