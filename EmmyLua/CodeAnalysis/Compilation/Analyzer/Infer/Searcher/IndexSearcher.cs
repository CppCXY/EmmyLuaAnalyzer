using EmmyLua.CodeAnalysis.Compilation.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer.Searcher;

public class IndexSearcher : LuaSearcher
{
    public override IEnumerable<ILuaType> SearchType(string className, SearchContext context)
    {
        var buildIn = context.Compilation.Builtin.FromName(className);
        if (buildIn is not null)
        {
            yield return buildIn;
        }

        var stubIndexImpl = context.Compilation.StubIndexImpl;
        if (stubIndexImpl.NamedTypeIndex.Get<ILuaNamedType>(className).FirstOrDefault() is { } ty)
        {
            yield return ty;
        }
    }

    public override IEnumerable<Declaration.Declaration> SearchMembers(string className, SearchContext context)
    {
        var stubIndexImpl = context.Compilation.StubIndexImpl;
        return stubIndexImpl.Members.Get<Declaration.Declaration>(className);
    }

    public override IEnumerable<Declaration.Declaration> SearchGenericParams(string className, SearchContext context)
    {
        var stubIndexImpl = context.Compilation.StubIndexImpl;
        return stubIndexImpl.GenericParams.Get<Declaration.Declaration>(className);
    }
}
