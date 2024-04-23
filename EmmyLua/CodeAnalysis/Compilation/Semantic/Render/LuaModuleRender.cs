using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Render;

public static class LuaModuleRender
{
    public static void RenderModule(LuaDocument document,  SearchContext context, StringBuilder sb)
    {
        var declarationTree = context.Compilation.GetDeclarationTree(document.Id);
        if (declarationTree is null)
        {
            return;
        }
        var exports = GetModuleExportElement(document, context);
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
                var returnStat = exportElement.AncestorsAndSelf.OfType<LuaReturnStatSyntax>().FirstOrDefault();
                if (returnStat is not null)
                {
                    LuaCommentRender.RenderStatComment(returnStat, sb);
                }
            }
        }
    }

    private static IEnumerable<LuaSyntaxElement> GetModuleExportElement(LuaDocument document, SearchContext context)
    {
        var result = new List<LuaSyntaxElement>();
        var mainBlock = document.SyntaxTree.SyntaxRoot.Block;
        if (mainBlock is null)
        {
            return result;
        }

        // var cfg = context.Compilation.GetControlFlowGraph(mainBlock);
        // if (cfg is null)
        // {
        //     return result;
        // }
        //
        // foreach (var prevNode in cfg.GetPredecessors(cfg.ExitNode))
        // {
        //     if (prevNode.Statements.LastOrDefault().ToNode(context) is LuaReturnStatSyntax returnStmt)
        //     {
        //         var exportElement = returnStmt.ExprList.FirstOrDefault();
        //         if (exportElement is not null)
        //         {
        //             result.Add(exportElement);
        //         }
        //     }
        // }

        return result;
    }
}
