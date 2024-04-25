using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Render;

public static class LuaModuleRender
{
    public static void RenderModule(LuaDocument document, SearchContext context, StringBuilder sb)
    {
        var declarationTree = context.Compilation.GetDeclarationTree(document.Id);
        if (declarationTree is null)
        {
            return;
        }

        var exports = context.Compilation.DbManager
            .GetModuleExportExprs(document.Id)
            .Select(it => it.ToNode(document));
        foreach (var exportElement in exports)
        {
            if (exportElement is LuaNameExprSyntax nameExpr)
            {
                var declaration = declarationTree.FindDeclaration(nameExpr, context);
                if (declaration is not null)
                {
                    LuaCommentRender.RenderDeclarationStatComment(declaration, context, sb);
                }
            }
            else
            {
                var returnStat = exportElement?.AncestorsAndSelf.OfType<LuaReturnStatSyntax>().FirstOrDefault();
                if (returnStat is not null)
                {
                    LuaCommentRender.RenderStatComment(returnStat, sb);
                }
            }
        }
    }
}
