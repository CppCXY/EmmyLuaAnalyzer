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
                        $"```lua\n(parameter) {parameter.Name}:{LuaTypeRender.RenderType(declarationType, context)}\n```");
                }
                else
                {
                    sb.Append($"```lua\n(parameter) {parameter.Name}\n```");
                }

                LuaCommentRender.RenderParamComment(parameter, sb);
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
