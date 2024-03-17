using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Render;

public class LuaRenderBuilder(SearchContext context)
{
    public string Render(LuaSyntaxElement element)
    {
        return element switch
        {
            LuaNameExprSyntax nameExpr => RenderNameExpr(nameExpr),
            LuaIndexExprSyntax indexExpr => RenderIndexExpr(indexExpr),
            LuaParamDefSyntax paramDef => RenderParamDef(paramDef),
            LuaLocalNameSyntax localName => RenderLocalName(localName),
            LuaLiteralExprSyntax literalExpr => RenderLiteralExpr(literalExpr),
            _ => string.Empty
        };
    }

    private string RenderNameExpr(LuaNameExprSyntax nameExpr)
    {
        var declarationTree = context.Compilation.GetDeclarationTree(nameExpr.Tree.Document.Id);
        var declaration = declarationTree?.FindDeclaration(nameExpr, context);
        var sb = new StringBuilder();
        if (declaration is not null)
        {
            RenderNameDeclaration(declaration, sb);
        }
        // TODO render comment

        return sb.ToString();
    }

    private string RenderIndexExpr(LuaIndexExprSyntax indexExpr)
    {
        var declarationTree = context.Compilation.GetDeclarationTree(indexExpr.Tree.Document.Id);
        var declaration = declarationTree?.FindDeclaration(indexExpr, context);
        var sb = new StringBuilder();
        if (declaration is not null)
        {
            switch (declaration)
            {
                case MethodLuaDeclaration method:
                {
                    sb.Append($"```lua\n(method) {method.Name}{LuaTypeRender.RenderFunc(method.DeclarationType, context)}\n```");
                    break;
                }
                default:
                {
                    sb.Append($"```lua\n(field) {declaration.Name}:{LuaTypeRender.RenderType(declaration.DeclarationType, context)}\n```");
                    break;
                }
            }
            var prefixType = context.Infer(indexExpr.PrefixExpr);
            if (!prefixType.Equals(Builtin.Unknown))
            {
                sb.Append($"\nin class `{LuaTypeRender.RenderType(prefixType, context)}`\n");
            }
        }

        return sb.ToString();
    }

    private string RenderParamDef(LuaParamDefSyntax paramDef)
    {
        var declarationTree = context.Compilation.GetDeclarationTree(paramDef.Tree.Document.Id);
        var declaration = declarationTree?.FindDeclaration(paramDef, context);
        var sb = new StringBuilder();
        if (declaration is not null)
        {
            RenderNameDeclaration(declaration, sb);
        }

        return sb.ToString();
    }

    private string RenderLocalName(LuaLocalNameSyntax localName)
    {
        var declarationTree = context.Compilation.GetDeclarationTree(localName.Tree.Document.Id);
        var declaration = declarationTree?.FindDeclaration(localName, context);
        var sb = new StringBuilder();
        if (declaration is not null)
        {
            RenderNameDeclaration(declaration, sb);
        }

        return sb.ToString();
    }

    private string RenderLiteralExpr(LuaLiteralExprSyntax literalExpr)
    {
        switch (literalExpr.Literal)
        {
            case LuaStringToken stringLiteral:
            {
                return $"'{stringLiteral.Value}' size:{stringLiteral.Value.Length}";
            }
            case LuaIntegerToken integerLiteral:
            {
                return integerLiteral.Value.ToString();
            }
            case LuaFloatToken floatToken:
            {
                return floatToken.ToString();
            }
            case LuaComplexToken complexToken:
            {
                return complexToken.ToString();
            }
            case LuaNilToken nilToken:
            {
                return "nil";
            }
        }

        return string.Empty;
    }

    private void RenderNameDeclaration(LuaDeclaration declaration, StringBuilder sb)
    {
        switch (declaration)
        {
            case LocalLuaDeclaration local:
            {
                sb.Append(
                    $"```lua\n(local) {local.Name}:{LuaTypeRender.RenderType(local.DeclarationType, context)}\n```");
                break;
            }
            case GlobalLuaDeclaration global:
            {
                sb.Append(
                    $"```lua\n(global) {global.Name}:{LuaTypeRender.RenderType(global.DeclarationType, context)}\n```");
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
                }

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
                    sb.Append($"```lua\n(parameter) {parameter.Name}\n\n```");
                }

                break;
            }
        }
    }

    private void RenderComment(LuaCommentSyntax comment, StringBuilder sb)
    {
        sb.Append($"\n{comment}\n");
    }
}
