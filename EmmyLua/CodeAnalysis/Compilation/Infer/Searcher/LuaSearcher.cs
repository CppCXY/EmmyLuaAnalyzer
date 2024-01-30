using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Infer.Searcher;

public abstract class LuaSearcher : ILuaSearcher
{
    public virtual IEnumerable<ILuaType> SearchType(string className, SearchContext context)
    {
        return Enumerable.Empty<ILuaNamedType>();
    }

    public virtual IEnumerable<Symbol.Symbol> SearchMembers(string className, SearchContext context)
    {
        return Enumerable.Empty<Symbol.Symbol>();
    }

    public virtual IEnumerable<GenericParameterDeclaration> SearchGenericParams(string className, SearchContext context)
    {
        return Enumerable.Empty<GenericParameterDeclaration>();
    }

    public virtual IEnumerable<ILuaType> SearchSupers(string className, SearchContext context)
    {
        return Enumerable.Empty<ILuaType>();
    }
}
