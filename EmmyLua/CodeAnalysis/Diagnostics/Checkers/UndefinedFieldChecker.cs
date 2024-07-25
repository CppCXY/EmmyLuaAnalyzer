using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.Diagnostics.Checkers;

public class UndefinedFieldChecker(LuaCompilation compilation)
    : DiagnosticCheckerBase(compilation, [
        DiagnosticCode.UndefinedField
    ])
{
    public override void Check(DiagnosticContext context)
    {
        var document = context.Document;
        foreach (var indexExpr in document.SyntaxTree.SyntaxRoot.Descendants.OfType<LuaIndexExprSyntax>())
        {
            var prefixType = context.SearchContext.Infer(indexExpr.PrefixExpr);
            if (prefixType.Equals(Builtin.Unknown) || prefixType is LuaArrayType)
            {
                continue;
            }

            var declaration = context.SearchContext.FindMember(prefixType, indexExpr);
            if (declaration.FirstOrDefault() is null && indexExpr.KeyElement is {} keyElement)
            {
                context.Report(
                    DiagnosticCode.UndefinedField,
                    $"Undefined field {indexExpr.Name}",
                    keyElement.Range
                );
            }
        }
    }
}
