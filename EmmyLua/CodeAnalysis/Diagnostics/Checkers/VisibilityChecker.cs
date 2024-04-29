using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Diagnostics.Checkers;

public class VisibilityChecker(LuaCompilation compilation)
    : DiagnosticCheckerBase(compilation, [
        DiagnosticCode.AccessPrivateMember
    ])
{
    public override void Check(DiagnosticContext context)
    {
        var semanticModel = Compilation.GetSemanticModel(context.Document.Id);
        if (semanticModel is null)
        {
            return;
        }

        var declarationTree = semanticModel.DeclarationTree;
        var searchContext = semanticModel.Context;
        var indexExprs = context.Document.SyntaxTree
            .SyntaxRoot.Descendants.OfType<LuaIndexExprSyntax>();
        foreach (var indexExpr in indexExprs)
        {
            var declaration = declarationTree.FindDeclaration(indexExpr, semanticModel.Context);
            var prefixExpr = indexExpr.PrefixExpr;
            if (prefixExpr is LuaNameExprSyntax { Name.RepresentText: "self" } && declaration is { IsPublic: false })
            {
                // TODO check super's protected and private
                continue;
            }

            var prefixType = semanticModel.Context.Infer(prefixExpr);
            if (prefixType.Equals(Builtin.Unknown) || prefixType.Equals(Builtin.Any))
            {
                continue;
            }

            if (declaration?.Visibility is DeclarationVisibility.Private or DeclarationVisibility.Protected &&
                indexExpr is { KeyElement: { } keyElement, Parent: not LuaStatSyntax })
            {
                context.Report(new Diagnostic(
                    DiagnosticSeverity.Error,
                    DiagnosticCode.AccessPrivateMember,
                    $"Cannot access private member '{indexExpr.Name}'",
                    keyElement.Range
                ));
            }
        }
    }
}
