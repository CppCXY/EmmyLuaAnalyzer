using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.Diagnostics.Checkers;

public class TypeNotFoundChecker(LuaCompilation compilation)
    : DiagnosticCheckerBase(compilation, [
        DiagnosticCode.TypeNotFound
    ])
{
    public override void Check(DiagnosticContext context)
    {
        var document = context.Document;
        foreach (var namedType in document.SyntaxTree.SyntaxRoot.Descendants.OfType<LuaDocNameTypeSyntax>())
        {
            if (namedType.Name is { RepresentText: {} representText })
            {
                var type = new LuaNamedType(namedType.DocumentId, representText);
                var typeInfo = Compilation.TypeManager.FindTypeInfo(type);
                if (typeInfo is null)
                {
                    context.Report(
                        DiagnosticCode.TypeNotFound,
                        $"Type {representText} not found.",
                        namedType.Range
                    );
                }
            }
        }
    }
}
