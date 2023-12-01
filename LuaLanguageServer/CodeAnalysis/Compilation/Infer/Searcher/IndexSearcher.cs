using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Infer.Searcher;

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
        if (stubIndexImpl.LuaTypeIndex.Get<ILuaNamedType>(name).FirstOrDefault() is { } ty)
        {
            type = ty;
            return true;
        }

        type = null;
        return false;
    }

    public override IEnumerable<LuaSymbol> SearchMembers(ILuaType type, SearchContext context)
    {
        var stubIndexImpl = context.Compilation.StubIndexImpl;
        return stubIndexImpl.Members.Get<LuaSymbol>(type);
    }

}
