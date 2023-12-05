using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class VirtualSymbol : LuaSymbol
{
    public ILuaType Type { get; }

    public VirtualSymbol(ILuaType type, ILuaType? containingType) : base(SymbolKind.VirtualSymbol, containingType)
    {
        Type = type;
    }

    public override ILuaType GetType(SearchContext context)
    {
        return Type;
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        return Enumerable.Empty<LuaLocation>();
    }
}
