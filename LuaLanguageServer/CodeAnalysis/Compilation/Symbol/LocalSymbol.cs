using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class LocalSymbol : LuaSymbol
{
    public string Name { get; }

    public LuaSyntaxElement Element { get; }

    public ILuaType? LuaType { get; }

    public LocalSymbol(LuaSyntaxElement element, string name, ILuaType luaType)
        : base(SymbolKind.LocalSymbol, null)
    {
        Name = name;
        Element = element;
        LuaType = luaType;
    }

    public override ILuaType GetType(SearchContext context)
    {
        return LuaType;
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        yield return Element.Location;
    }
}
