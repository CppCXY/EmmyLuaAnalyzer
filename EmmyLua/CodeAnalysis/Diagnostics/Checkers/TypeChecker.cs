using EmmyLua.CodeAnalysis.Compilation;

namespace EmmyLua.CodeAnalysis.Diagnostics.Checkers;

public class TypeChecker(LuaCompilation compilation) : DiagnosticCheckerBase(compilation, [])
{
    public override void Check(DiagnosticContext context)
    {
        foreach (var node in context.Document.SyntaxTree.SyntaxRoot.Descendants)
        {
            // TODO
        }
    }
}
