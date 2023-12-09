using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class LocalSymbol(LuaSyntaxElement element, string name, ILuaType luaType)
    : LuaSymbol(SymbolKind.LocalSymbol, null)
{
    public string Name { get; } = name;

    public LuaSyntaxElement Element { get; } = element;

    public ILuaType? LuaType { get; } = luaType;

    public override ILuaType GetType(SearchContext context)
    {
        return LuaType;
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        yield return Element.Location;
    }
}
