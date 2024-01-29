using EmmyLua.CodeAnalysis.Compilation.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Infer.Searcher;

public interface ILuaSearcher
{
    IEnumerable<ILuaType> SearchType(string className, SearchContext context);

    IEnumerable<Symbol.Symbol> SearchMembers(string className, SearchContext context);

    IEnumerable<Symbol.Symbol> SearchGenericParams(string className, SearchContext context);

    IEnumerable<ILuaType> SearchSupers(string className, SearchContext context);
}
