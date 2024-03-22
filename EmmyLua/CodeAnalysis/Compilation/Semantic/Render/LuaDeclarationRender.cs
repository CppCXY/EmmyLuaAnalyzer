using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Render;

public static class LuaDeclarationRender
{
    public static void RenderDeclaration(LuaDeclaration declaration, SearchContext context, StringBuilder sb)
    {
        switch (declaration)
        {
            case LocalLuaDeclaration local:
            {
                RenderLocalDeclaration(local, context, sb);
                break;
            }
            case GlobalLuaDeclaration global:
            {
                RenderGlobalDeclaration(global, context, sb);
                break;
            }
            case MethodLuaDeclaration method:
            {
                var isLocal = method.MethodDef?.IsLocal ?? false;
                if (isLocal)
                {
                    sb.Append(
                        $"```lua\nlocal function {method.Name}{LuaTypeRender.RenderFunc(method.DeclarationType, context)}\n```");
                }
                else
                {
                    sb.Append(
                        $"```lua\nfunction {method.Name}{LuaTypeRender.RenderFunc(method.DeclarationType, context)}\n```");
                    RenderInClass(method.SyntaxElement, context, sb);
                }

                LuaCommentRender.RenderStatComment(declaration, sb);
                break;
            }
            case ParameterLuaDeclaration parameter:
            {
                if (parameter.DeclarationType is { } declarationType)
                {
                    sb.Append(
                        $"```lua\n(parameter) {parameter.Name} : {LuaTypeRender.RenderType(declarationType, context)}\n```");
                }
                else
                {
                    sb.Append($"```lua\n(parameter) {parameter.Name}\n```");
                }

                LuaCommentRender.RenderParamComment(parameter, sb);
                break;
            }
            case DocFieldLuaDeclaration docField:
            {
                if (docField.FieldDef is { } fieldDef)
                {
                    sb.Append(
                        $"```lua\n {docField.Name} : {LuaTypeRender.RenderType(docField.DeclarationType, context)}\n```");
                    LuaCommentRender.RenderDocFieldComment(docField, sb);
                }
                // TODO: handle TypedFieldDef
                // else if (docField.TypedFieldDef is { } typedFieldDef)
                // {
                //     sb.Append(
                //         $"```lua\n {docField.Name} : {LuaTypeRender.RenderType(docField.DeclarationType, context)}\n```");
                //     LuaCommentRender.RenderDocFieldComment(docField, sb);
                // }

                break;
            }
            case TableFieldLuaDeclaration tableField:
            {
                var constExpr = string.Empty;
                if (tableField.TableField is { IsValue: false, Value: LuaLiteralExprSyntax expr })
                {
                    switch (expr.Literal)
                    {
                        case LuaStringToken stringLiteral:
                        {
                            constExpr = $" = '{stringLiteral.Value}'";
                            break;
                        }
                        case LuaIntegerToken integerLiteral:
                        {
                            constExpr = $" = {integerLiteral.Value}";
                            break;
                        }
                        case LuaFloatToken floatToken:
                        {
                            constExpr = $" = {floatToken.Value}";
                            break;
                        }
                        case LuaComplexToken complexToken:
                        {
                            constExpr = $" = {complexToken}";
                            break;
                        }
                    }
                }
                sb.Append(
                    $"```lua\n(field) {tableField.Name} : {LuaTypeRender.RenderType(tableField.DeclarationType, context)}{constExpr}\n```");
                LuaCommentRender.RenderTableFieldComment(tableField, sb);
                break;
            }
            case NamedTypeLuaDeclaration namedTypeLuaDeclaration:
            {
                var declarationType = namedTypeLuaDeclaration.DeclarationType;
                var typeDescription = "class";
                if (declarationType is LuaNamedType namedType)
                {
                    var detailType = namedType.GetDetailType(context);
                    if (detailType.IsAlias)
                    {
                        typeDescription = "alias";
                    }
                    else if (detailType.IsEnum)
                    {
                        typeDescription = "enum";
                    }
                    else if (detailType.IsInterface)
                    {
                        typeDescription = "interface";
                    }
                }

                sb.Append(
                    $"```lua\n{typeDescription} {namedTypeLuaDeclaration.Name} : {LuaTypeRender.RenderType(namedTypeLuaDeclaration.DeclarationType, context)}\n```");
                // LuaCommentRender.RenderStatComment(declaration, sb);
                break;
            }
            case TypeIndexDeclaration typeIndexDeclaration:
            {
                var field = typeIndexDeclaration.Field;
                if (field is { TypeField: { } typeField, Type: { } type })
                {
                    var keyType = context.Infer(typeField);
                    var valueTType = context.Infer(type);
                    sb.Append(
                        $"```lua\n(index) [{LuaTypeRender.RenderType(keyType, context)}] : {LuaTypeRender.RenderType(valueTType, context)}\n```");
                }

                break;
            }
            case IndexLuaDeclaration indexLuaDeclaration:
            {
                var indexExpr = indexLuaDeclaration.IndexExpr;
                sb.Append(
                    $"```lua\n(field) {indexExpr.Name} : {LuaTypeRender.RenderType(indexLuaDeclaration.DeclarationType, context)}\n```");
                LuaCommentRender.RenderStatComment(indexLuaDeclaration, sb);
                break;
            }
        }
    }

    private static void RenderInClass(LuaSyntaxElement? element, SearchContext context, StringBuilder sb)
    {
        if (element is LuaIndexExprSyntax indexExpr)
        {
            var prefixType = context.Infer(indexExpr.PrefixExpr);
            if (!prefixType.Equals(Builtin.Unknown))
            {
                RenderBelongType(prefixType, context, sb);
            }
        }
    }

    private static void RenderBelongType(LuaType prefixType, SearchContext context, StringBuilder sb)
    {
        if (!prefixType.Equals(Builtin.Unknown))
        {
            var parentTypeDescription = "class";
            if (prefixType is LuaNamedType namedType)
            {
                var detailType = namedType.GetDetailType(context);
                if (detailType.IsAlias)
                {
                    parentTypeDescription = "alias";
                }
                else if (detailType.IsEnum)
                {
                    parentTypeDescription = "enum";
                }
                else if (detailType.IsInterface)
                {
                    parentTypeDescription = "interface";
                }
            }

            sb.Append($"\nin {parentTypeDescription} `{LuaTypeRender.RenderType(prefixType, context)}`");
        }
    }

    public static void RenderLocalDeclaration(LocalLuaDeclaration local, SearchContext context, StringBuilder sb)
    {
        var attrib = "";
        if (local.IsConst)
        {
            attrib = " <const>";
        }
        else if (local.IsClose)
        {
            attrib = " <close>";
        }

        if (local.IsTypeDefine)
        {
            sb.Append(
                $"```lua\nlocal {local.Name}{attrib} : {LuaTypeRender.RenderDefinedType(local.DeclarationType, context)}\n```");
            LuaCommentRender.RenderStatComment(local, sb);
        }
        else
        {
            sb.Append(
                $"```lua\nlocal {local.Name}{attrib} : {LuaTypeRender.RenderType(local.DeclarationType, context)}\n```");
            LuaCommentRender.RenderStatComment(local, sb);
        }
    }

    public static void RenderGlobalDeclaration(GlobalLuaDeclaration global, SearchContext context, StringBuilder sb)
    {
        if (global.IsTypeDefine)
        {
            sb.Append(
                $"```lua\nglobal {global.Name}: {LuaTypeRender.RenderDefinedType(global.DeclarationType, context)}\n```");
            LuaCommentRender.RenderStatComment(global, sb);
        }
        else
        {
            sb.Append(
                $"```lua\nglobal {global.Name} : {LuaTypeRender.RenderType(global.DeclarationType, context)}\n```");
            LuaCommentRender.RenderStatComment(global, sb);
        }
    }
}
