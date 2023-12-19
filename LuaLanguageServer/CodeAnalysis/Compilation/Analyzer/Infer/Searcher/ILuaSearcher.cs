using LuaLanguageServer.CodeAnalysis.Compilation.Type;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer.Searcher;

public interface ILuaSearcher
{
    bool TrySearchLuaType(string name, SearchContext context, out ILuaNamedType? type);

    IEnumerable<Declaration.Declaration> SearchMembers(ILuaType type, SearchContext context);
}
