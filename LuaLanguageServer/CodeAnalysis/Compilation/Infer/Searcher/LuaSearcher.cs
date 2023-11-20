using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Infer.Searcher;

public abstract class LuaSearcher : ILuaSearcher
{
    public bool TrySearchLuaType(string name, SearchContext context, out ILuaNamedType? type)
    {
        type = null;
        return false;
    }

    public IEnumerable<LuaSymbol> SearchMembers(ILuaType type, SearchContext context)
    {
        return Enumerable.Empty<LuaSymbol>();
    }

    public IEnumerable<Declaration.Declaration> SearchDeclarations(LuaSyntaxElement element, SearchContext context)
    {
        return Enumerable.Empty<Declaration.Declaration>();
    }
}
