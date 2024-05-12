using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.LanguageServer.Completion.CompleteProvider;

public class SelfMemberProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        if (context.TriggerToken?.Parent is not LuaNameExprSyntax)
        {
            return;
        }

        var luaFuncStat = context.TriggerToken.Ancestors.OfType<LuaFuncStatSyntax>().FirstOrDefault();

        if (luaFuncStat is { IsColonFunc: true, IndexExpr.PrefixExpr: { } selfExpr })
        {
            var selfType = context.SemanticModel.Context.Infer(selfExpr);
            var members = context.SemanticModel.Context.GetMembers(selfType);
            foreach (var member in members)
            {
                if (member.Info.DeclarationType is LuaMethodType { ColonDefine: true })
                {
                    context.CreateCompletion($"self:{member.Name}", member.Info.DeclarationType)
                        .WithData(member.Info.Ptr.Stringify)
                        .WithCheckDeclaration(member)
                        .AddToContext();
                }
                else
                {
                    context.CreateCompletion($"self.{member.Name}", member.Info.DeclarationType)
                        .WithData(member.Info.Ptr.Stringify)
                        .WithCheckDeclaration(member)
                        .AddToContext();
                }
            }
        }
    }
}