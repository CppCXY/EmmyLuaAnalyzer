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
}
