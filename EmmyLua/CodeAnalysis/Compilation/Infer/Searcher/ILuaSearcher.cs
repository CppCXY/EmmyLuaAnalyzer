using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Infer.Searcher;

public interface ILuaSearcher
{
    IEnumerable<LuaType> SearchType(string className, SearchContext context);

    IEnumerable<Declaration> SearchMembers(string className, SearchContext context);

    IEnumerable<GenericParameterDeclaration> SearchGenericParams(string className, SearchContext context);

    IEnumerable<LuaType> SearchSupers(string className, SearchContext context);
}
