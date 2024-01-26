using EmmyLua.CodeAnalysis.Compile.Diagnostic;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic;

public class SemanticModel(LuaCompilation compilation, LuaDocument document)
{
    public LuaCompilation Compilation { get; } = compilation;

    public LuaDocument Document { get; } = document;

    public Symbol.Symbol? GetSymbol(LuaSyntaxElement element)
    {
        var symbolTree = Compilation.GetSymbolTree(Document.Id);
        if (symbolTree?.FindDeclaration(element, Compilation.SearchContext) is { } symbol)
        {
            return symbol;
        }

        return null;
    }

    public IEnumerable<Diagnostic> GetDiagnostic()
    {
        var tree = Compilation.GetSyntaxTree(Document.Id);

        return tree?.Diagnostics.Select(it => it.WithLocation(
                   tree.Document.GetLocation(it.Range)
               ))
               ?? Enumerable.Empty<Diagnostic>();
    }
}
