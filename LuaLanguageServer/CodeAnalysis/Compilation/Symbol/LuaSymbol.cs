using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public abstract class LuaSymbol(SymbolKind kind, ILuaType? containingType) : ILuaSymbol
{
    public SymbolKind Kind { get; } = kind;

    public ILuaType? ContainingType { get; } = containingType;

    public LuaSymbol(ILuaType containingType) : this(SymbolKind.Unknown, containingType)
    {
    }

    public abstract ILuaType GetType(SearchContext context);

    public abstract IEnumerable<LuaLocation> GetLocations(SearchContext context);
}
