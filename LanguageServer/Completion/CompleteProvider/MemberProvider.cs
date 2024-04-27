using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LanguageServer.Completion.CompleteProvider;

public class MemberProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        var triggerToken = context.TriggerToken;
        switch (triggerToken)
        {
            case { Kind: LuaTokenKind.TkDot or LuaTokenKind.TkColon }:
            {
                AddCompletionNormal(context);
                break;
            }
            case { Kind: LuaTokenKind.TkLeftBracket }:
            {
                AddCompletionInBracket(context);
                break;
            }
            case { Kind: LuaTokenKind.TkString }:
            {
                AddCompletionInString(context);
                break;
            }
            case LuaNameToken { Parent: LuaIndexExprSyntax }:
            {
                AddCompletionNormal(context);
                break;
            }
        }
    }

    private void AddCompletionNormal(CompleteContext context)
    {
        if (context.TriggerToken?.Parent is not LuaIndexExprSyntax indexExpr)
        {
            return;
        }

        var prefixType = context.SemanticModel.Context.Infer(indexExpr.PrefixExpr);
        if (!prefixType.Equals(Builtin.Table))
        {
            var colon = indexExpr.IsColonIndex;
            foreach (var member in context.SemanticModel.Context.GetMembers(prefixType))
            {
                context.CreateCompletion(member.Name, member.DeclarationType)
                    .WithColon(colon)
                    .WithData(member.Ptr.Stringify)
                    .WithDotCheckBracketLabel(indexExpr)
                    .WithCheckDeprecated(member)
                    .AddToContext();
            }
        }
    }

    private void AddCompletionInString(CompleteContext context)
    {
        if (context.TriggerToken?.Parent?.Parent is not LuaIndexExprSyntax indexExpr)
        {
            return;
        }

        var prefixType = context.SemanticModel.Context.Infer(indexExpr.PrefixExpr);
        if (!prefixType.Equals(Builtin.Table))
        {
            foreach (var member in context.SemanticModel.Context.GetMembers(prefixType))
            {
                context.CreateCompletion(member.Name, member.DeclarationType)
                    .WithData(member.Ptr.Stringify)
                    .WithCheckDeprecated(member)
                    .AddToContext();
            }
        }
    }

    private void AddCompletionInBracket(CompleteContext context)
    {
        if (context.TriggerToken?.Parent is not LuaIndexExprSyntax indexExpr)
        {
            return;
        }

        var prefixType = context.SemanticModel.Context.Infer(indexExpr.PrefixExpr);
        if (!prefixType.Equals(Builtin.Table))
        {
            foreach (var member in context.SemanticModel.Context.GetMembers(prefixType))
            {
                if (member.Name.StartsWith("["))
                {
                    var label = member.Name[1..^1];
                    context.CreateCompletion(label, member.DeclarationType)
                        .WithData(member.Ptr.Stringify)
                        .WithCheckDeprecated(member)
                        .AddToContext();
                }
                else
                {
                    context.CreateCompletion($"\"{member.Name}\"", member.DeclarationType)
                        .WithData(member.Ptr.Stringify)
                        .WithCheckDeprecated(member)
                        .AddToContext();
                }
            }
        }
    }
}