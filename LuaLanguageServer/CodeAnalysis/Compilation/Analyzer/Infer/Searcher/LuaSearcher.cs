using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer.Searcher;

public abstract class LuaSearcher : ILuaSearcher
{
    public virtual  bool TrySearchLuaType(string name, SearchContext context, out ILuaNamedType? type)
    {
        type = null;
        return false;
    }

    public virtual IEnumerable<LuaSymbol> SearchMembers(ILuaType type, SearchContext context)
    {
        return Enumerable.Empty<LuaSymbol>();
    }

    public virtual IEnumerable<Declaration.Declaration> SearchDeclarations(LuaSyntaxElement element, SearchContext context)
    {
        return Enumerable.Empty<Declaration.Declaration>();
    }
}
