using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class VirtualSymbol(ILuaType type, ILuaType? containingType)
    : LuaSymbol(SymbolKind.VirtualSymbol, containingType)
{
    public ILuaType Type { get; } = type;

    public override ILuaType GetType(SearchContext context)
    {
        return Type;
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        return Enumerable.Empty<LuaLocation>();
    }
}
