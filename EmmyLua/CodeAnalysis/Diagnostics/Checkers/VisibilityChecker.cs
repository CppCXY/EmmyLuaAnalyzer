using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
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
        // var searchContext = semanticModel.Context;
        var indexExprList = context.Document.SyntaxTree
            .SyntaxRoot.Descendants.OfType<LuaIndexExprSyntax>();
        foreach (var indexExpr in indexExprList)
        {
            if (indexExpr.Parent is LuaFuncStatSyntax || indexExpr.KeyElement is null)
            {
                continue;
            }

            var declaration = context.SearchContext.FindDeclaration(indexExpr);
            if (declaration is null || declaration.IsPublic)
            {
                continue;
            }

            if (declaration is LuaDeclaration { Info.Ptr.UniqueId: { } id } && id == indexExpr.UniqueId)
            {
                continue;
            }

            var prefixExpr = indexExpr.PrefixExpr;
            if (prefixExpr is null)
            {
                continue;
            }

            if (declaration.IsPackage && (declaration.DocumentId != indexExpr.DocumentId))
            {
                context.Report(
                    DiagnosticCode.AccessPackageMember,
                    $"Cannot access package member '{indexExpr.Name}'",
                    indexExpr.KeyElement.Range
                );

                continue;
            }

            var envElement = FindSourceOrClosure(indexExpr);
            if (declaration.IsPrivate)
            {
                if (envElement is LuaSourceSyntax)
                {
                    context.Report(
                        DiagnosticCode.AccessPrivateMember,
                        $"Cannot access private member '{indexExpr.Name}'",
                        indexExpr.KeyElement.Range
                    );

                    continue;
                }
                var parentType = context.SearchContext.Compilation.Db.QueryParentType(declaration.UniqueId);
                var parentTable = context.SearchContext.Infer(prefixExpr);
                if (!parentTable.Equals(parentType))
                {
                    context.Report(
                        DiagnosticCode.AccessPrivateMember,
                        $"Cannot access private member '{indexExpr.Name}'",
                        indexExpr.KeyElement.Range
                    );
                }
            }
            else if (declaration.IsProtected)
            {
                if (envElement is LuaSourceSyntax)
                {
                    context.Report(
                        DiagnosticCode.AccessProtectedMember,
                        $"Cannot access protected member '{indexExpr.Name}'",
                        indexExpr.KeyElement.Range
                    );

                    continue;
                }

                var parentType = context.SearchContext.Compilation.Db.QueryParentType(declaration.UniqueId);
                var parentTable = context.SearchContext.Infer(prefixExpr);
                if (!parentTable.SubTypeOf(parentType, context.SearchContext))
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

    private static LuaSyntaxElement FindSourceOrClosure(LuaSyntaxElement element)
    {
        foreach (var ancestor in element.Ancestors)
        {
            if (ancestor is LuaSourceSyntax)
            {
                return ancestor;
            }
            else if (ancestor is LuaClosureExprSyntax)
            {
                return ancestor;
            }
        }

        return element;
    }
}
