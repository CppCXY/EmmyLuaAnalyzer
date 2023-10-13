using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public interface ILuaSymbol : IEquatable<ILuaSymbol?>
{
    public ILuaSymbol? ContainingSymbol { get; }

    public SymbolKind Kind { get; }

    IEnumerable<LuaLocation> Locations { get; }

    public string DisplayName { get; }
}

