using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Type;

namespace EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Infer.Searcher;

public interface ILuaSearcher
{
    IEnumerable<ILuaType> SearchType(string className, SearchContext context);

    IEnumerable<Declaration.Declaration> SearchMembers(string className, SearchContext context);

    IEnumerable<Declaration.Declaration> SearchGenericParams(string className, SearchContext context);

    IEnumerable<ILuaType> SearchSupers(string className, SearchContext context);
}
