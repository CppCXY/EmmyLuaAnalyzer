using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class IndexFieldSymbol : LuaSymbol
{
    public long Index { get; }

    public LuaSyntaxElement? TypeElement { get; }

    public LuaSyntaxElement Element { get; }

    public IndexFieldSymbol(LuaSyntaxElement element, long index, LuaSyntaxElement? typeElement, ILuaType containingType)
        : base(SymbolKind.IndexFieldSymbol, containingType)
    {
        Element = element;
        Index = index;
        TypeElement = typeElement;
    }

    public override ILuaType GetType(SearchContext context)
    {
        return context.Infer(TypeElement);
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        yield return Element.Location;
    }
}
