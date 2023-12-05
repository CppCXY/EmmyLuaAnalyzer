using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class NamedSymbol : LuaSymbol
{
    public string Name { get; }

    public ILuaType Type { get; }

    public LuaSyntaxElement Element { get; }

    public NamedSymbol(LuaSyntaxElement element, string name, ILuaType type, ILuaType containingType)
        : base(SymbolKind.NamedSymbol, containingType)
    {
        Name = name;
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
