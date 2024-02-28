using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Infer.Searcher;

public abstract class LuaSearcher : ILuaSearcher
{
    public virtual IEnumerable<LuaType> SearchType(string className, SearchContext context) =>
        Enumerable.Empty<LuaNamedType>();

    public virtual IEnumerable<Declaration> SearchMembers(string className, SearchContext context) =>
        Enumerable.Empty<Declaration>();

    public virtual IEnumerable<GenericParameterDeclaration>
        SearchGenericParams(string className, SearchContext context) => Enumerable.Empty<GenericParameterDeclaration>();

    public virtual IEnumerable<LuaType> SearchSupers(string className, SearchContext context) =>
        Enumerable.Empty<LuaType>();
}
