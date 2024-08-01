using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Diagnostics.Checkers;

public class VisibilityChecker(LuaCompilation compilation)
    : DiagnosticCheckerBase(compilation, [
        DiagnosticCode.AccessPrivateMember,
        DiagnosticCode.AccessProtectedMember,
        DiagnosticCode.AccessPackageMember
    ])
{
    public override void Check(DiagnosticContext context)
    {
        var searchContext = context.SearchContext;
        var indexExprList = context.Document.SyntaxTree
            .SyntaxRoot.Descendants.OfType<LuaIndexExprSyntax>();
        foreach (var indexExpr in indexExprList)
        {
            if (indexExpr.Parent is LuaFuncStatSyntax || indexExpr.KeyElement is null)
            {
                continue;
            }

            var symbol = context.SearchContext.FindDeclaration(indexExpr);
            if (symbol is null || symbol.IsPublic)
            {
                continue;
            }

            if (symbol is { UniqueId: { } id } && id == indexExpr.UniqueId)
            {
                continue;
            }

            if (searchContext.IsVisible(indexExpr, symbol))
            {
                continue;
            }

            if (symbol.IsPackage)
            {
                context.Report(
                    DiagnosticCode.AccessPackageMember,
                    $"Cannot access package member '{indexExpr.Name}'",
                    indexExpr.KeyElement.Range
                );
            }
            else if (symbol.IsPrivate)
            {
                context.Report(
                    DiagnosticCode.AccessPrivateMember,
                    $"Cannot access private member '{indexExpr.Name}'",
                    indexExpr.KeyElement.Range
                );
            }
            else if (symbol.IsProtected)
            {
                context.Report(
                    DiagnosticCode.AccessProtectedMember,
                    $"Cannot access protected member '{indexExpr.Name}'",
                    indexExpr.KeyElement.Range
                );
            }
        }
    }
}
