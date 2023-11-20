using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Infer.Searcher;

public interface ILuaSearcher
{
    bool TrySearchLuaType(string name, SearchContext context, out ILuaNamedType? type);

    IEnumerable<LuaSymbol> SearchMembers(ILuaType type, SearchContext context);
}
