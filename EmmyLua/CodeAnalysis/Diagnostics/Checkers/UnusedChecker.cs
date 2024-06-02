using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Diagnostics.Checkers;

public class UnusedChecker(LuaCompilation compilation) : DiagnosticCheckerBase(compilation, [DiagnosticCode.Unused])
{
    public override void Check(DiagnosticContext context)
    {
        var declarationTree = context.SearchContext.Compilation.GetDeclarationTree(context.Document.Id);
        var declarations = declarationTree?.RootScope?.Descendants;
        if (declarations is null)
        {
            return;
        }

        var localOrParamDeclarations = new HashSet<IDeclaration>();
        foreach (var luaDeclaration in declarations)
        {
            if (luaDeclaration.Info is LocalInfo or ParamInfo)
            {
                localOrParamDeclarations.Add(luaDeclaration);
            }
        }

        LocalOrParamUnusedCheck(localOrParamDeclarations, context);
    }

    private void LocalOrParamUnusedCheck(
        HashSet<IDeclaration> declarationSet,
        DiagnosticContext context)
    {
        var nameExprs = context
            .Document
            .SyntaxTree
            .SyntaxRoot
            .Descendants
            .OfType<LuaNameExprSyntax>();

        foreach (var nameExpr in nameExprs)
        {
            if (context.SearchContext.FindDeclaration(nameExpr) is { } declaration)
            {
                declarationSet.Remove(declaration);
            }
        }

        foreach (var luaDeclaration in declarationSet)
        {
            if (luaDeclaration is LuaDeclaration { Info: { } info } && info.Ptr.ToNode(context.Document) is { } node)
            {
                context.Report(
                    DiagnosticCode.Unused,
                    "Unused variable",
                    node.Range,
                    DiagnosticTag.Unnecessary
                );
            }
        }
    }
}
