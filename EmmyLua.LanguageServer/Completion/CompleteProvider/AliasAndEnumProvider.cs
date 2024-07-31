using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;
using EmmyLua.CodeAnalysis.Type.Manager.TypeInfo;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;


namespace EmmyLua.LanguageServer.Completion.CompleteProvider;

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
        else if (trigger is LuaNameToken { Parent: LuaCallArgListSyntax callArgs2 })
        {
            AddFuncParamCompletion(callArgs2, context);
        }
        else if (trigger is LuaStringToken { Parent.Parent: LuaCallArgListSyntax callArgs3 })
        {
            AddFuncParamCompletion(callArgs3, context);
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
        foreach (var methodType in context.SemanticModel.Context.FindCallableType(prefixType))
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
                var paramType = param.Type;
                if (paramType is LuaNamedType namedType)
                {
                    var typeInfo = context.SemanticModel.Compilation.TypeManager.FindTypeInfo(namedType);
                    if (typeInfo?.Kind == NamedTypeKind.Alias)
                    {
                        AddAliasParamCompletion(typeInfo, context);
                    }
                    else if (typeInfo?.Kind == NamedTypeKind.Enum)
                    {
                        AddEnumParamCompletion(typeInfo, context);
                    }
                }
                else if (paramType is LuaAggregateType aggregateType)
                {
                    AddAggregateTypeCompletion(aggregateType, context);
                }
                else if (paramType is LuaUnionType unionType)
                {
                    AddUnionTypeCompletion(unionType, context);
                }
            }
        }
    }

    private void AddAliasParamCompletion(TypeInfo typeInfo, CompleteContext context)
    {
        var baseType = typeInfo.BaseType;
        if (baseType is LuaAggregateType aggregateType)
        {
            AddAggregateTypeCompletion(aggregateType, context);
        }
        else if (baseType is LuaUnionType unionType)
        {
            AddUnionTypeCompletion(unionType, context);
        }
    }

    private void AddEnumParamCompletion(TypeInfo typeInfo, CompleteContext context)
    {
        if (typeInfo.Declarations is null)
        {
            return;
        }
        
        foreach (var field in typeInfo.Declarations.Values)
        {
            context.Add(new CompletionItem
            {
                Label = field.Name,
                Kind = CompletionItemKind.EnumMember,
            });
        }
    }

    private void AddAggregateTypeCompletion(LuaAggregateType aggregateType, CompleteContext context)
    {
        foreach (var declaration in aggregateType.Declarations.OfType<LuaSymbol>())
        {
            if (declaration.Info.Ptr.ToNode(context.SemanticModel.Context) is LuaDocLiteralTypeSyntax literalType)
            {
                var detail = string.Empty;
                if (literalType.Description is { Details: { } details })
                {
                    detail = string.Join("\n", details.Select(d => d.RepresentText.Trim('#', '@')));
                }

                if (literalType is { IsString: true, String: { } stringLiteral })
                {
                    var label = stringLiteral.RepresentText;
                    // compact emmylua old alias
                    if (declaration.Type is LuaStringLiteralType stringLiteralType
                        && (stringLiteralType.Content.StartsWith('\'') || stringLiteralType.Content.StartsWith('"')))
                    {
                        label = stringLiteralType.Content;
                    }

                    if (context.TriggerToken is not LuaStringToken)
                    {
                        label = $"\"{label}\"";
                    }

                    context.Add(new CompletionItem
                    {
                        Label = label,
                        Kind = CompletionItemKind.EnumMember,
                        Detail = detail
                    });
                }
                else if (declaration.Type is LuaIntegerLiteralType intLiteralType)
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

    private void AddUnionTypeCompletion(LuaUnionType unionType, CompleteContext context)
    {
        foreach (var luaType in unionType.UnionTypes)
        {
            if (luaType is LuaStringLiteralType stringLiteralType)
            {
                var label = stringLiteralType.Content;
                if (stringLiteralType.Content.StartsWith('\'') || stringLiteralType.Content.StartsWith('"'))
                {
                    label = stringLiteralType.Content.Trim('\'', '"');
                }

                if (context.TriggerToken is not LuaStringToken)
                {
                    label = $"\"{label}\"";
                }

                context.Add(new CompletionItem
                {
                    Label = label,
                    Kind = CompletionItemKind.EnumMember,
                });
            }
            else if (luaType is LuaIntegerLiteralType intLiteralType)
            {
                context.Add(new CompletionItem
                {
                    Label = intLiteralType.Value.ToString(),
                    Kind = CompletionItemKind.EnumMember,
                });
            }
        }
    }
}