using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Render.Renderer;

internal static class LuaModuleRenderer
{
    public static void RenderModule(LuaDocument document, LuaRenderContext renderContext)
    {
        var declarationTree = renderContext.SearchContext.Compilation.GetDeclarationTree(document.Id);
        if (declarationTree is null)
        {
            return;
        }

        var exports = renderContext.SearchContext.Compilation.DbManager
            .GetModuleExportExprs(document.Id)
            .Select(it => it.ToNode(document));
        foreach (var exportElement in exports)
        {
            if (exportElement is LuaNameExprSyntax nameExpr)
            {
                var declaration = declarationTree.FindDeclaration(nameExpr, renderContext.SearchContext);
                if (declaration is not null)
                {
                    LuaCommentRenderer.RenderDeclarationStatComment(declaration, renderContext);
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
