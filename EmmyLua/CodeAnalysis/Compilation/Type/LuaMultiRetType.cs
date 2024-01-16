using EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaMultiRetType(List<ILuaType> rets) : LuaType(TypeKind.MultiRet)
{
    public List<ILuaType> Returns => rets;

    public static LuaMultiRetType FromType(ILuaType? ty)
    {
        if (ty is LuaMultiRetType multiRetType)
        {
            return multiRetType;
        }

        return ty is null ? new LuaMultiRetType([]) : new LuaMultiRetType([ty]);
    }

    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return Enumerable.Empty<Declaration>();
    }

    public ILuaType? GetRetType(int index)
    {
        return index < rets.Count ? rets[index] : null;
    }
}
