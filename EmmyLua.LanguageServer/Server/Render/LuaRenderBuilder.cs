﻿using System.Globalization;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;
using EmmyLua.CodeAnalysis.Type.Types;
using EmmyLua.LanguageServer.Server.Render.Renderer;

namespace EmmyLua.LanguageServer.Server.Render;

public class LuaRenderBuilder(SearchContext context)
{
    public string Render(LuaSyntaxElement? element, LuaRenderFeature feature)
    {
        var renderContext = new LuaRenderContext(context, feature);
        switch (element)
        {
            case LuaNameExprSyntax or LuaIndexExprSyntax
                or LuaParamDefSyntax or LuaLocalNameSyntax
                or LuaCallExprSyntax or LuaTableFieldSyntax
                or LuaDocNameTypeSyntax or LuaDocGenericTypeSyntax
                or LuaDocTagNamedTypeSyntax or LuaDocFieldSyntax:
            {
                RenderElement(element, renderContext);
                break;
            }
            case LuaLiteralExprSyntax literalExpr:
            {
                RenderLiteralExpr(literalExpr, renderContext);
                break;
            }
            case LuaDocTagParamSyntax paramSyntax:
            {
                RenderTagParam(paramSyntax, renderContext);
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

    public string RenderDeclaration(LuaSymbol symbol, LuaRenderFeature feature)
    {
        var renderContext = new LuaRenderContext(context, feature);
        LuaDeclarationRenderer.RenderDeclaration(symbol, renderContext);
        return renderContext.GetText();
    }

    private void RenderElement(LuaSyntaxElement element, LuaRenderContext renderContext)
    {
        var declaration = renderContext.SearchContext.FindDeclaration(element);
        if (declaration is { } luaDeclaration)
        {
            LuaDeclarationRenderer.RenderDeclaration(luaDeclaration, renderContext);
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
                    && searchContext.Compilation.Project.Features.RequireLikeFunction.Contains(funcName))
                {
                    renderContext.WrapperLuaAppend($"module {display}");
                    var moduleDocument = searchContext.Compilation.Project.ModuleManager.FindModule(stringLiteral.Value);
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

    private void RenderTagParam(LuaDocTagParamSyntax paramSyntax, LuaRenderContext renderContext)
    {
        renderContext.EnableAliasRender();
        var searchContext = renderContext.SearchContext;
        var name = paramSyntax.Name?.RepresentText;
        if (name is not null)
        {
            renderContext.WrapperLua(() =>
            {
                renderContext.Append($"(parameter) {name} : ");
                if (paramSyntax.Type is { } type)
                {
                    var luaType = searchContext.Infer(type);
                    LuaTypeRenderer.RenderType(luaType, renderContext);
                }
            });

            if (paramSyntax.Description is { } description)
            {
                renderContext.AppendLine();
                renderContext.Append(description.CommentText);
            }
        }
    }
}
