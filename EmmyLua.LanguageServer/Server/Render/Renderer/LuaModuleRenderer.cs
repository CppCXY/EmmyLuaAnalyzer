using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.LanguageServer.Server.Render.Renderer;

public static class LuaModuleRenderer
{
    public static void RenderModule(LuaDocument document, LuaRenderContext renderContext)
    {
        var exports = renderContext.SearchContext.Compilation.Db
            .QueryModuleReturns(document.Id)
            .Select(it => it.ToNode(document));
        foreach (var exportElement in exports)
        {
            if (exportElement is LuaNameExprSyntax nameExpr)
            {
                var declaration =  renderContext.SearchContext.FindDeclaration(nameExpr);
                if (declaration is { } luaDeclaration)
                {
                    LuaCommentRenderer.RenderDeclarationStatComment(luaDeclaration, renderContext);
                }
            }
            else
            {
                var returnStat = exportElement?.AncestorsAndSelf.OfType<LuaReturnStatSyntax>().FirstOrDefault();
                if (returnStat is not null)
                {
                    LuaCommentRenderer.RenderStatComment(returnStat, renderContext);
                }
            }
        }
    }
}
