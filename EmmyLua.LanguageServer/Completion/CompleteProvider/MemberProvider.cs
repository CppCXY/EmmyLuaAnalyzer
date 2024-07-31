using System.Text;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;
using EmmyLua.LanguageServer.Util;


namespace EmmyLua.LanguageServer.Completion.CompleteProvider;

public class MemberProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        var triggerToken = context.TriggerToken;
        switch (triggerToken)
        {
            case { Kind: LuaTokenKind.TkDot or LuaTokenKind.TkColon }:
            {
                if (triggerToken.Parent is LuaFuncStatSyntax funcStatSyntax)
                {
                    AddFuncOverrideCompletion(context, funcStatSyntax);
                    return;
                }

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

        if (indexExpr.Parent is LuaFuncStatSyntax funcStatSyntax)
        {
            AddFuncOverrideCompletion(context, funcStatSyntax);
            return;
        }

        var prefixType = context.SemanticModel.Context.Infer(indexExpr.PrefixExpr);
        var colon = indexExpr.IsColonIndex;
        foreach (var member in context.SemanticModel.Context.GetMembers(prefixType))
        {
            context.CreateCompletion(member.Name, member.Type)
                .WithColon(colon)
                .WithData(member.RelationInformation)
                .WithDotCheckBracketLabel(indexExpr)
                .WithCheckDeclaration(member)
                .AddToContext();
        }
    }

    private void AddFuncOverrideCompletion(CompleteContext context, LuaFuncStatSyntax funcStatSyntax)
    {
        if (funcStatSyntax.IndexExpr is null)
        {
            return;
        }

        var indexExpr = funcStatSyntax.IndexExpr;
        var prefixType = context.SemanticModel.Context.Infer(indexExpr.PrefixExpr);
        if (prefixType is not LuaNamedType namedType)
        {
            return;
        }

        var colon = indexExpr.IsColonIndex;
        foreach (var member in context.SemanticModel
                     .Context.GetSuperMembers(namedType))
        {
            if (member.Type is LuaMethodType methodType)
            {
                var insertRange = context.TriggerToken!.Range.ToLspRange(context.SemanticModel.Document);
                if (context.TriggerToken is { Kind: LuaTokenKind.TkDot or LuaTokenKind.TkColon })
                {
                    insertRange = insertRange with
                    {
                        Start = new Position(insertRange.Start.Line, insertRange.Start.Character + 1)
                    };
                }

                var replaceRange = insertRange;
                if (funcStatSyntax.ClosureExpr?.ParamList?.FirstChildToken(LuaTokenKind.TkRightParen) is { } token)
                {
                    var col = context.SemanticModel.Document.GetCol(token.Range.EndOffset);
                    replaceRange = new(insertRange.Start, new Position(insertRange.End.Line, col));
                }

                var textOrReplaceEdit = new TextEditOrInsertReplaceEdit(new TextEdit()
                {
                    NewText =
                        $"{member.Name}{MakeSignature(methodType.MainSignature, methodType.ColonDefine, colon)}",
                    Range = replaceRange,
                });

                context.CreateCompletion($"override {member.Name}", member.Type)
                    .WithColon(colon)
                    .WithTextEditOrReplaceEdit(textOrReplaceEdit)
                    .WithData(member.RelationInformation)
                    .WithDotCheckBracketLabel(indexExpr)
                    .WithCheckDeclaration(member)
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
        if (!prefixType.IsSameType(Builtin.Table, context.SemanticModel.Context))
        {
            foreach (var member in context.SemanticModel.Context.GetMembers(prefixType))
            {
                context.CreateCompletion(member.Name, member.Type)
                    .WithData(member.RelationInformation)
                    .WithCheckDeclaration(member)
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
        if (!prefixType.IsSameType(Builtin.Table, context.SemanticModel.Context))
        {
            foreach (var member in context.SemanticModel.Context.GetMembers(prefixType))
            {
                if (member.Name.StartsWith("["))
                {
                    var label = member.Name[1..^1];
                    context.CreateCompletion(label, member.Type)
                        .WithData(member.RelationInformation)
                        .WithCheckDeclaration(member)
                        .AddToContext();
                }
                else
                {
                    context.CreateCompletion($"\"{member.Name}\"", member.Type)
                        .WithData(member.RelationInformation)
                        .WithCheckDeclaration(member)
                        .AddToContext();
                }
            }
        }
    }

    private string MakeSignature(LuaSignature signature, bool colonDefine, bool colonCall)
    {
        var sb = new StringBuilder();
        sb.Append('(');
        var parameters = signature.Parameters;
        switch ((colonDefine, colon: colonCall))
        {
            case (true, false):
            {
                sb.Append("self");
                if (parameters.Count > 0)
                {
                    sb.Append(", ");
                }

                break;
            }
            case (false, true):
            {
                parameters = parameters.Skip(1).ToList();
                break;
            }
        }

        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            sb.Append(parameter.Name);
            if (i < parameters.Count - 1)
            {
                sb.Append(", ");
            }
        }

        sb.Append(')');
        return sb.ToString();
    }
}