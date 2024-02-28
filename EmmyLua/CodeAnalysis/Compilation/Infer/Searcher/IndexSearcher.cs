using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Infer.Searcher;

public class IndexSearcher : LuaSearcher
{
    public override IEnumerable<LuaType> SearchType(string className, SearchContext context)
    {
        var buildIn = context.Compilation.Builtin.FromName(className);
        if (buildIn is not null)
        {
            yield return buildIn;
        }

        var stubIndexImpl = context.Compilation.ProjectIndex;
        if (stubIndexImpl.NamedType.Get<LuaNamedType>(className).FirstOrDefault() is { } ty)
        {
            yield return ty;
        }
    }

    public override IEnumerable<Declaration> SearchMembers(string className, SearchContext context)
    {
        var stubIndexImpl = context.Compilation.ProjectIndex;
        return stubIndexImpl.Members.Get<Declaration>(className);
    }

    public override IEnumerable<GenericParameterDeclaration> SearchGenericParams(string className, SearchContext context)
    {
        var stubIndexImpl = context.Compilation.ProjectIndex;
        return stubIndexImpl.GenericParam.Get(className);
    }
}
