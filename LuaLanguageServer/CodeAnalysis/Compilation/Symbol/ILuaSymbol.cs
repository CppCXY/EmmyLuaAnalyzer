using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public interface ILuaSymbol
{
    public SymbolKind Kind { get; }

    public ILuaType? ContainingType { get; }

    public ILuaType GetType(SearchContext context);

    public IEnumerable<LuaLocation> GetLocations(SearchContext context);
}
