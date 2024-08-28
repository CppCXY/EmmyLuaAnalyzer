using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Diagnostics.Checkers;

public class UndefinedFieldChecker(LuaCompilation compilation)
    : DiagnosticCheckerBase(compilation, [
        DiagnosticCode.UndefinedField,
        DiagnosticCode.InjectFieldFail
    ])
{
    public override void Check(DiagnosticContext context)
    {
        var document = context.Document;
        foreach (var indexExpr in document.SyntaxTree.SyntaxRoot.Descendants.OfType<LuaIndexExprSyntax>())
        {
            var prefixType = context.SearchContext.Infer(indexExpr.PrefixExpr);
            if (prefixType.SubTypeOf(Builtin.Unknown, context.SearchContext) || prefixType is LuaArrayType)
            {
                continue;
            }

            var luaSymbol = context.SearchContext.FindMember(prefixType, indexExpr);
            if (luaSymbol is null && indexExpr.KeyElement is { } keyElement)
            {
                if (indexExpr.Parent is LuaAssignStatSyntax { Assign: { } assign }
                    && assign.Position > indexExpr.Position
                   )
                {
                    context.Report(
                        DiagnosticCode.InjectFieldFail,
                        $"Inject field fail {indexExpr.Name}",
                        keyElement.Range
                    );
                    continue;
                }

                context.Report(
                    DiagnosticCode.UndefinedField,
                    $"Undefined field {indexExpr.Name}",
                    keyElement.Range
                );
            }
        }
    }
}
