using System.Globalization;
using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
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
            LuaTableFieldSyntax tableField => RenderTableField(tableField),
            LuaDocNameTypeSyntax docNameType => RenderDocNameType(docNameType),
            LuaDocGenericTypeSyntax docGenericType => RenderDocGenericType(docGenericType),
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
            LuaDeclarationRender.RenderDeclaration(declaration, context, sb);
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
            LuaDeclarationRender.RenderDeclaration(declaration, context, sb);
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
            LuaDeclarationRender.RenderDeclaration(declaration, context, sb);
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
            LuaDeclarationRender.RenderDeclaration(declaration, context, sb);
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

                var display = $"\"{preview}\"";
                if (literalExpr.Parent?.Parent is LuaCallExprSyntax { Name: { } funcName }
                    && context.Compilation.Workspace.Features.RequireLikeFunction.Contains(funcName))
                {
                    var sb = new StringBuilder();
                    sb.Append($"```lua\nmodule {display}\n```");
                    var moduleDocument = context.Compilation.Workspace.ModuleGraph.FindModule(stringLiteral.Value);
                    if (moduleDocument is not null)
                    {
                        LuaModuleRender.RenderModule(moduleDocument, sb, context);
                    }

                    display = sb.ToString();
                }
                else
                {
                    display = $"```lua\n{display}\n```";
                }

                return display;
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
            LuaDeclarationRender.RenderDeclaration(declaration, context, sb);
        }

        return sb.ToString();
    }

    private string RenderTableField(LuaTableFieldSyntax tableField)
    {
        var declarationTree = context.Compilation.GetDeclarationTree(tableField.Tree.Document.Id);
        var declaration = declarationTree?.FindDeclaration(tableField, context);
        var sb = new StringBuilder();
        if (declaration is not null)
        {
            LuaDeclarationRender.RenderDeclaration(declaration, context, sb);
        }

        return sb.ToString();
    }

    private string RenderDocNameType(LuaDocNameTypeSyntax docNameType)
    {
        var declarationTree = context.Compilation.GetDeclarationTree(docNameType.Tree.Document.Id);
        var declaration = declarationTree?.FindDeclaration(docNameType, context);
        var sb = new StringBuilder();
        if (declaration is not null)
        {
            LuaDeclarationRender.RenderDeclaration(declaration, context, sb);
        }

        return sb.ToString();
    }

    private string RenderDocGenericType(LuaDocGenericTypeSyntax docGenericType)
    {
        var declarationTree = context.Compilation.GetDeclarationTree(docGenericType.Tree.Document.Id);
        var declaration = declarationTree?.FindDeclaration(docGenericType, context);
        var sb = new StringBuilder();
        if (declaration is not null)
        {
            LuaDeclarationRender.RenderDeclaration(declaration, context, sb);
        }

        return sb.ToString();
    }
}
