using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public interface ISymbol
{
    public ILuaType ContainingType { get; }

    public ILuaType GetType(SearchContext context);

    public bool MatchKey(IndexKey key, SearchContext context);
}
