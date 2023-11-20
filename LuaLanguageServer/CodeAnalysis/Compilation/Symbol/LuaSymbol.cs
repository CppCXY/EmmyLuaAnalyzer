using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public abstract class LuaSymbol : ILuaSymbol
{
    public SymbolKind Kind { get; }

    public ILuaType? ContainingType { get; }

    public LuaSymbol(SymbolKind kind, ILuaType? containingType)
    {
        Kind = kind;
        ContainingType = containingType;
    }

    public LuaSymbol(ILuaType containingType)
    {
        Kind = SymbolKind.Unknown;
        ContainingType = containingType;
    }

    public abstract ILuaType GetType(SearchContext context);

    public abstract IEnumerable<LuaLocation> GetLocations(SearchContext context);
}
