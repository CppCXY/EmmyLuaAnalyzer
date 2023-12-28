using LuaLanguageServer.CodeAnalysis.Compilation.Type;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer.Searcher;

public class IndexSearcher : LuaSearcher
{
    public override bool TrySearchLuaType(string name, SearchContext context, out ILuaNamedType? type)
    {
        var buildIn = context.Compilation.Builtin.FromName(name);
        if (buildIn is not null)
        {
            type = buildIn;
            return true;
        }

        var stubIndexImpl = context.Compilation.StubIndexImpl;
        if (stubIndexImpl.NamedTypeIndex.Get<ILuaNamedType>(name).FirstOrDefault() is { } ty)
        {
            type = ty;
            return true;
        }

        type = null;
        return false;
    }

    public override IEnumerable<Declaration.Declaration> SearchMembers(ILuaType type, SearchContext context)
    {
        var stubIndexImpl = context.Compilation.StubIndexImpl;
        // return stubIndexImpl.Members.Get<LuaSymbol>(type);
        throw new NotImplementedException();
    }

}
