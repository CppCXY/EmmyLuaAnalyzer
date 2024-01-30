using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Infer.Searcher;

public class IndexSearcher : LuaSearcher
{
    public override IEnumerable<ILuaType> SearchType(string className, SearchContext context)
    {
        var buildIn = context.Compilation.Builtin.FromName(className);
        if (buildIn is not null)
        {
            yield return buildIn;
        }

        var stubIndexImpl = context.Compilation.Stub;
        if (stubIndexImpl.NamedTypeIndex.Get<ILuaNamedType>(className).FirstOrDefault() is { } ty)
        {
            yield return ty;
        }
    }

    public override IEnumerable<Symbol.Symbol> SearchMembers(string className, SearchContext context)
    {
        var stubIndexImpl = context.Compilation.Stub;
        return stubIndexImpl.Members.Get<Symbol.Symbol>(className);
    }

    public override IEnumerable<GenericParameterDeclaration> SearchGenericParams(string className, SearchContext context)
    {
        var stubIndexImpl = context.Compilation.Stub;
        return stubIndexImpl.NamedTypeGenericParams.Get(className);
    }
}
