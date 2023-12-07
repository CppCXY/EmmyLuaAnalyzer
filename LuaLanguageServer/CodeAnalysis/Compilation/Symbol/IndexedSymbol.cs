using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class IndexedSymbol : LuaSymbol
{
    public long Index { get; }

    public ILuaType Type { get; }

    public LuaSyntaxElement Element { get; }

    public IndexedSymbol(LuaSyntaxElement element, long index, ILuaType type, ILuaType containingType)
        : base(SymbolKind.NamedSymbol, containingType)
    {
        Index = index;
        Element = element;
        Type = type;
    }

    public override ILuaType GetType(SearchContext context)
    {
        return Type;
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        yield return Element.Location;
    }
}
