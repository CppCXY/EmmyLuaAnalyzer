using LuaLanguageServer.CodeAnalysis.Compilation.Type;


namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer.Searcher;

public abstract class LuaSearcher : ILuaSearcher
{
    public virtual IEnumerable<ILuaType> SearchType(string className, SearchContext context)
    {
        return Enumerable.Empty<ILuaNamedType>();
    }

    public virtual IEnumerable<Declaration.Declaration> SearchMembers(string className, SearchContext context)
    {
        return Enumerable.Empty<Declaration.Declaration>();
    }

    public virtual IEnumerable<Declaration.Declaration> SearchGenericParams(string className, SearchContext context)
    {
        return Enumerable.Empty<Declaration.Declaration>();
    }

    public virtual IEnumerable<ILuaType> SearchSupers(string className, SearchContext context)
    {
        return Enumerable.Empty<ILuaType>();
    }
}
