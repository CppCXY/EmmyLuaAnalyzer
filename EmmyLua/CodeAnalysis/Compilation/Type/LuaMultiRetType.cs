using EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaMultiRetType(List<ILuaType> rets) : LuaType(TypeKind.MultiRet)
{
    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return Enumerable.Empty<Declaration>();
    }

    public ILuaType? GetRetType(int index)
    {
        return index < rets.Count ? rets[index] : null;
    }
}
