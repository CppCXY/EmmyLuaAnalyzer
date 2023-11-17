using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Infer.Searcher;

public interface ILuaSearcher
{
    bool TrySearchType(string name, SearchContext context, out ILuaType type);

    IEnumerable<LuaTypeMember> SearchMembers(ILuaType type, SearchContext context);

    IEnumerable<Declaration.Declaration> SearchDeclarations(LuaSyntaxElement element, SearchContext context);
}
