using System.Globalization;
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
            LuaCallExprSyntax callExpr => RenderCallExpr(callExpr),
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
            RenderDeclaration(declaration, sb);
            var declarationElement = declaration.SyntaxElement;
            var comments =
                declarationElement?.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault()?.Comments;
            LuaCommentRender.RenderCommentDescription(comments, sb);
        }

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
                    sb.Append(
                        $"```lua\n(method) {method.Name}{LuaTypeRender.RenderFunc(method.DeclarationType, context)}\n```");
                    break;
                }
                default:
                {
                    sb.Append(
                        $"```lua\n(field) {declaration.Name}:{LuaTypeRender.RenderType(declaration.DeclarationType, context)}\n```");
                    break;
                }
            }

            var prefixType = context.Infer(indexExpr.PrefixExpr);
            if (!prefixType.Equals(Builtin.Unknown))
            {
                RenderBelongType(prefixType, sb);
            }

            var declarationElement = declaration.SyntaxElement;
            var comments =
                declarationElement?.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault()?.Comments;
            LuaCommentRender.RenderCommentDescription(comments, sb);
        }

        return sb.ToString();
    }

    private void RenderSeparator(StringBuilder sb)
    {
        if (sb.Length > 0)
        {
            sb.Append("\n___\n");
        }
    }

    private void RenderBelongType(LuaType prefixType, StringBuilder sb)
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

    private string RenderParamDef(LuaParamDefSyntax paramDef)
    {
        var declarationTree = context.Compilation.GetDeclarationTree(paramDef.Tree.Document.Id);
        var declaration = declarationTree?.FindDeclaration(paramDef, context);
        var sb = new StringBuilder();
        if (declaration is not null)
        {
            RenderDeclaration(declaration, sb);
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
            RenderDeclaration(declaration, sb);

            var declarationElement = declaration.SyntaxElement;
            var comments =
                declarationElement?.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault()?.Comments;
            LuaCommentRender.RenderCommentDescription(comments, sb);
        }

        return sb.ToString();
    }

    private string RenderLiteralExpr(LuaLiteralExprSyntax literalExpr)
    {
        switch (literalExpr.Literal)
        {
            case LuaStringToken stringLiteral:
            {
                var enumerator = StringInfo.GetTextElementEnumerator(stringLiteral.Value);
                var preview = new StringBuilder();
                var count = 0;
                while (enumerator.MoveNext() && count < 100)
                {
                    preview.Append(enumerator.GetTextElement());
                    count++;
                }

                if (count == 100)
                {
                    preview.Append("...");
                }

                return $"'{preview}' size:{stringLiteral.Value.Length}";
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

    private string RenderCallExpr(LuaCallExprSyntax callExpr)
    {
        var declarationTree = context.Compilation.GetDeclarationTree(callExpr.Tree.Document.Id);
        var declaration = declarationTree?.FindDeclaration(callExpr, context);
        var sb = new StringBuilder();
        if (declaration is not null)
        {
            switch (declaration)
            {
                case MethodLuaDeclaration method:
                {
                    sb.Append(
                        $"```lua\n(method) {method.Name}{LuaTypeRender.RenderFunc(method.DeclarationType, context)}\n```");
                    break;
                }
                default:
                {
                    RenderDeclaration(declaration, sb);
                    break;
                }
            }

            var declarationElement = declaration.SyntaxElement;
            var comments =
                declarationElement?.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault()?.Comments;
            LuaCommentRender.RenderCommentDescription(comments, sb);
        }

        return sb.ToString();
    }

    private void RenderDeclaration(LuaDeclaration declaration, StringBuilder sb)
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
}
