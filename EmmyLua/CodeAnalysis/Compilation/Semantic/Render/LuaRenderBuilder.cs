using System.Globalization;
using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render.Renderer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Render;

public class LuaRenderBuilder(SearchContext context)
{
    public string Render(LuaSyntaxElement element, LuaRenderFeature feature)
    {
        var renderContext = new LuaRenderContext(context, feature);
        switch (element)
        {
            case LuaNameExprSyntax or LuaIndexExprSyntax
                or LuaParamDefSyntax or LuaLocalNameSyntax
                or LuaCallExprSyntax or LuaTableFieldSyntax
                or LuaDocNameTypeSyntax or LuaDocGenericTypeSyntax
                or LuaDocTagNamedTypeSyntax:
            {
                RenderElement(element, renderContext);
                break;
            }
            case LuaLiteralExprSyntax literalExpr:
            {
                RenderLiteralExpr(literalExpr, renderContext);
                break;
            }
        }

        return renderContext.GetText();
    }

    public string RenderType(LuaType type, LuaRenderFeature feature)
    {
        var renderContext = new LuaRenderContext(context, feature);
        LuaTypeRenderer.RenderType(type, renderContext);
        return renderContext.GetText();
    }

    public string RenderModule(LuaDocument document, LuaRenderFeature feature)
    {
        var renderContext = new LuaRenderContext(context, feature);
        LuaModuleRenderer.RenderModule(document, renderContext);
        return renderContext.GetText();
    }

    private void RenderElement(LuaSyntaxElement element, LuaRenderContext renderContext)
    {
        var declarationTree = renderContext.SearchContext.Compilation.GetDeclarationTree(element.DocumentId);
        var declaration = declarationTree?.FindDeclaration(element, renderContext.SearchContext);
        if (declaration is not null)
        {
            LuaDeclarationRenderer.RenderDeclaration(declaration, renderContext);
        }
    }

    private void RenderLiteralExpr(LuaLiteralExprSyntax literalExpr, LuaRenderContext renderContext)
    {
        var feature = renderContext.Feature;
        var searchContext = renderContext.SearchContext;
        switch (literalExpr.Literal)
        {
            case LuaStringToken stringLiteral:
            {
                var preview = stringLiteral.Value;
                if (stringLiteral.Value.Length > feature.MaxStringPreviewLength)
                {
                    preview = stringLiteral.Value[..feature.MaxStringPreviewLength] + "...";
                }

                var display = $"\"{preview}\"";
                if (literalExpr.Parent?.Parent is LuaCallExprSyntax {Name: { } funcName}
                    && searchContext.Compilation.Workspace.Features.RequireLikeFunction.Contains(funcName))
                {
                    renderContext.WrapperLuaAppend($"module {display}");
                    var moduleDocument = searchContext.Compilation.Workspace.ModuleGraph.FindModule(stringLiteral.Value);
                    if (moduleDocument is not null)
                    {
                        LuaModuleRenderer.RenderModule(moduleDocument, renderContext);
                    }
                }
                else
                {
                    renderContext.WrapperLuaAppend(display);
                }

                break;
            }
            case LuaIntegerToken integerLiteral:
            {
                renderContext.Append(integerLiteral.Value.ToString());
                break;
            }
            case LuaFloatToken floatToken:
            {
                renderContext.Append(floatToken.Value.ToString(CultureInfo.CurrentCulture));
                break;
            }
            case LuaComplexToken complexToken:
            {
                renderContext.Append(complexToken.Value);
                break;
            }
            case LuaNilToken nilToken:
            {
                renderContext.Append("nil");
                break;
            }
        }
    }
}
