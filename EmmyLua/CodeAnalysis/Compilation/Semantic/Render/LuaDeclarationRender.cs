using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Kind;
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
                var luaFunc = method.FuncStatPtr.ToNode(context);
                var isLocal = luaFunc?.IsLocal ?? false;
                if (isLocal)
                {
                    sb.Append(
                        $"```lua\nlocal function {method.Name}{LuaTypeRender.RenderFunc(method.DeclarationType, context)}\n```");
                }
                else
                {
                    sb.Append(
                        $"```lua\nfunction {method.Name}{LuaTypeRender.RenderFunc(method.DeclarationType, context)}\n```");
                    if (method.IndexExprPtr.ToNode(context) is { } indexExpr)
                    {
                        RenderInClass(indexExpr, context, sb);
                    }
                }

                LuaCommentRender.RenderDeclarationStatComment(declaration, context, sb);
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

                LuaCommentRender.RenderParamComment(parameter, context, sb);
                break;
            }
            case DocFieldLuaDeclaration docField:
            {
                if (docField.FieldDefPtr.ToNode(context) is { } fieldDef)
                {
                    var visibilityText = fieldDef.Visibility switch
                    {
                        VisibilityKind.Public => "public ",
                        VisibilityKind.Protected => "protected ",
                        VisibilityKind.Private => "private ",
                        _ => ""
                    };

                    sb.Append(
                        $"```lua\n(field) {visibilityText}{docField.Name} : {LuaTypeRender.RenderType(docField.DeclarationType, context)}\n```");
                    LuaCommentRender.RenderDocFieldComment(docField, context, sb);
                }

                break;
            }
            case TableFieldLuaDeclaration tableFieldDeclaration:
            {
                var constExpr = string.Empty;
                if (tableFieldDeclaration.TableFieldPtr.ToNode(context) is
                    { IsValue: false, Value: LuaLiteralExprSyntax expr })
                {
                    constExpr = RenderLiteral(expr);
                }

                sb.Append(
                    $"```lua\n(field) {tableFieldDeclaration.Name} : {LuaTypeRender.RenderType(tableFieldDeclaration.DeclarationType, context)}{constExpr}\n```");
                LuaCommentRender.RenderTableFieldComment(tableFieldDeclaration, context, sb);
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
                if (typeIndexDeclaration is { KeyType: { } keyType, ValueType: { } valueType })
                {
                    sb.Append(
                        $"```lua\n(index) [{LuaTypeRender.RenderType(keyType, context)}] : {LuaTypeRender.RenderType(valueType, context)}\n```");
                }

                break;
            }
            case IndexLuaDeclaration indexLuaDeclaration:
            {
                var literalText = string.Empty;
                var valueExpr = indexLuaDeclaration.ValueExprPtr.ToNode(context);
                if (valueExpr is LuaLiteralExprSyntax literalExpr)
                {
                    literalText = RenderLiteral(literalExpr);
                }

                var indexExpr = indexLuaDeclaration.IndexExprPtr.ToNode(context);
                sb.Append(
                    $"```lua\n(field) {indexExpr?.Name} : {LuaTypeRender.RenderType(indexLuaDeclaration.DeclarationType, context)}{literalText}\n```");
                LuaCommentRender.RenderDeclarationStatComment(indexLuaDeclaration, context, sb);
                break;
            }
        }
    }

    private static void RenderInClass(LuaIndexExprSyntax indexExpr, SearchContext context, StringBuilder sb)
    {
        var prefixType = context.Infer(indexExpr.PrefixExpr);
        if (!prefixType.Equals(Builtin.Unknown))
        {
            RenderBelongType(prefixType, context, sb);
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

    private static void RenderLocalDeclaration(LocalLuaDeclaration local, SearchContext context, StringBuilder sb)
    {
        var localName = local.LocalNamePtr.ToNode(context);

        var attrib = "";
        if (localName is { Attribute.IsConst: true })
        {
            attrib = " <const>";
        }
        else if (localName is { Attribute.IsClose: true })
        {
            attrib = " <close>";
        }

        if (local.IsTypeDefine)
        {
            sb.Append(
                $"```lua\nlocal {local.Name}{attrib} : {LuaTypeRender.RenderDefinedType(local.DeclarationType, context)}\n```");
            LuaCommentRender.RenderDeclarationStatComment(local, context, sb);
        }
        else
        {
            sb.Append(
                $"```lua\nlocal {local.Name}{attrib} : {LuaTypeRender.RenderType(local.DeclarationType, context)}\n```");
            LuaCommentRender.RenderDeclarationStatComment(local, context, sb);
        }
    }

    private static void RenderGlobalDeclaration(GlobalLuaDeclaration global, SearchContext context, StringBuilder sb)
    {
        if (global.IsTypeDefine)
        {
            sb.Append(
                $"```lua\nglobal {global.Name}: {LuaTypeRender.RenderDefinedType(global.DeclarationType, context)}\n```");
            LuaCommentRender.RenderDeclarationStatComment(global, context, sb);
        }
        else
        {
            sb.Append(
                $"```lua\nglobal {global.Name} : {LuaTypeRender.RenderType(global.DeclarationType, context)}\n```");
            LuaCommentRender.RenderDeclarationStatComment(global, context, sb);
        }
    }

    private static string RenderLiteral(LuaLiteralExprSyntax expr)
    {
        switch (expr.Literal)
        {
            case LuaStringToken stringLiteral:
            {
                return $" = '{stringLiteral.Value}'";
            }
            case LuaIntegerToken integerLiteral:
            {
                return $" = {integerLiteral.Value}";
            }
            case LuaFloatToken floatToken:
            {
                return $" = {floatToken.Value}";
            }
            case LuaComplexToken complexToken:
            {
                return $" = {complexToken}";
            }
        }

        return string.Empty;
    }

    private static void RenderMethodDeclaration(MethodLuaDeclaration method, SearchContext context, StringBuilder sb)
    {
        var luaFunc = method.FuncStatPtr.ToNode(context);
        var isLocal = luaFunc?.IsLocal ?? false;
        if (isLocal)
        {
            sb.Append(
                $"```lua\nlocal function {method.Name}{LuaTypeRender.RenderFunc(method.DeclarationType, context)}\n```");
        }
        else
        {
            sb.Append(
                $"```lua\nfunction {method.Name}{LuaTypeRender.RenderFunc(method.DeclarationType, context)}\n```");
            if (method.IndexExprPtr.ToNode(context) is { } indexExpr)
            {
                RenderInClass(indexExpr, context, sb);
            }
        }

        LuaCommentRender.RenderDeclarationStatComment(method, context, sb);
    }
}
