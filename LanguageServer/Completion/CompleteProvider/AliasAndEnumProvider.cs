using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.DetailType;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion.CompleteProvider;

public class AliasAndEnumProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        var trigger = context.TriggerToken;
        if (trigger is
            {
                Kind: LuaTokenKind.TkLeftParen
                or LuaTokenKind.TkComma
                or LuaTokenKind.TkString
                or LuaTokenKind.TkWhitespace,
                Parent: LuaCallArgListSyntax callArgs
            })
        {
            AddFuncParamCompletion(callArgs, context);
        }
        else if (trigger is LuaNameToken {Parent: LuaCallArgListSyntax callArgs2})
        {
            AddFuncParamCompletion(callArgs2, context);
        }
    }

    private void AddFuncParamCompletion(LuaCallArgListSyntax callArgList, CompleteContext context)
    {
        var trigger = context.TriggerToken!;
        if (callArgList.Parent is not LuaCallExprSyntax callExpr)
        {
            return;
        }

        var activeParam = callArgList.ChildTokens(LuaTokenKind.TkComma)
            .Count(comma => comma.Position <= trigger.Position);

        var prefixType = context.SemanticModel.Context.Infer(callExpr.PrefixExpr);
        TypeHelper.Each(prefixType, type =>
        {
            if (type is LuaMethodType methodType)
            {
                var colonDefine = methodType.ColonDefine;
                var colonCall = (callExpr.PrefixExpr as LuaIndexExprSyntax)?.IsColonIndex ?? false;
                switch ((colonDefine, colonCall))
                {
                    case (true, false):
                    {
                        activeParam--;
                        break;
                    }
                    case (false, true):
                    {
                        activeParam++;
                        break;
                    }
                }

                if (activeParam >= 0 && activeParam < methodType.MainSignature.Parameters.Count)
                {
                    var param = methodType.MainSignature.Parameters[activeParam];
                    var paramType = param.Info.DeclarationType;
                    if (paramType is LuaNamedType namedType)
                    {
                        var detailType = namedType.GetDetailType(context.SemanticModel.Context);
                        if (detailType.IsAlias && detailType is AliasDetailType aliasDetailType)
                        {
                            AddAliasParamCompletion(aliasDetailType, context);
                        }
                        else if (detailType.IsEnum && detailType is EnumDetailType enumDetailType)
                        {
                            AddEnumParamCompletion(enumDetailType, context);
                        }
                    }
                    else if (paramType is LuaAggregateType aggregateType)
                    {
                        AddAggregateTypeCompletion(aggregateType, context);
                    }
                }
            }
        });
    }

    private void AddAliasParamCompletion(AliasDetailType aliasDetailType, CompleteContext context)
    {
        if (aliasDetailType.OriginType is LuaAggregateType aggregateType)
        {
            AddAggregateTypeCompletion(aggregateType, context);
        }
    }

    private void AddEnumParamCompletion(EnumDetailType enumDetailType, CompleteContext context)
    {
        var enumName = enumDetailType.Name;
        var members = context.SemanticModel.Compilation.DbManager
            .GetMembers(enumName);

        foreach (var field in members)
        {
            context.Add(new CompletionItem
            {
                Label = $"{enumName}.{field.Name}",
                Kind = CompletionItemKind.EnumMember,
            });
        }
    }

    private void AddAggregateTypeCompletion(LuaAggregateType aggregateType, CompleteContext context)
    {
        foreach (var declaration in aggregateType.Declarations)
        {
            if (declaration.Info.Ptr.ToNode(context.SemanticModel.Context) is LuaDocLiteralTypeSyntax literalType)
            {
                var detail = string.Empty;
                if (literalType.Description is {Details: {} details  })
                {
                    detail = string.Join("\n", details.Select(d => d.RepresentText.Trim('#', '@')));
                }
                if (literalType is {IsString: true, String: {} stringLiteral })
                {
                    var label = stringLiteral.RepresentText;
                    // compact emmylua old alias
                    if (declaration.Info.DeclarationType is LuaStringLiteralType stringLiteralType
                        && (stringLiteralType.Content.StartsWith('\'') || stringLiteralType.Content.StartsWith('"')))
                    {
                        label = stringLiteralType.Content;
                    }
                    context.Add(new CompletionItem
                    {
                        Label = label,
                        Kind = CompletionItemKind.EnumMember,
                        Detail = detail
                    });
                }
                else if (declaration.Info.DeclarationType is LuaIntegerLiteralType intLiteralType)
                {
                    context.Add(new CompletionItem
                    {
                        Label = intLiteralType.Value.ToString(),
                        Kind = CompletionItemKind.EnumMember,
                        Detail = detail
                    });
                }
            }
        }
    }
}