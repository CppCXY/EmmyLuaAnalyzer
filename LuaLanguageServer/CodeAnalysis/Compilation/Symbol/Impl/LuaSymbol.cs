using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class LuaSymbol : ILuaSymbol
{
    public LuaSymbol(SymbolKind kind)
        : this(string.Empty, kind)
    {
    }

    public LuaSymbol(string name, SymbolKind kind)
    {
        Kind = kind;
        Name = name;
    }


    public ILuaSymbol? ContainingSymbol { get; set; } = null;

    public SymbolKind Kind { get; }

    public string Name { get; protected set; }

    public virtual IEnumerable<LuaLocation> Locations => throw new NotImplementedException();

    public virtual bool SubTypeOf(ILuaSymbol symbol, SearchContext context)
    {
        if (symbol.Kind == SymbolKind.Unknown) return false;

        // Handle unions, subtype if subtype of any of the union components.
        // if (other is TyUnion) return other.getChildTypes().any { type -> subTypeOf(type, context, strict) }

        // Classes are equal
        return this == symbol;
    }

    public virtual string DisplayName => Name;

    public virtual IEnumerable<ILuaSymbol> Members => Enumerable.Empty<ILuaSymbol>();
}
