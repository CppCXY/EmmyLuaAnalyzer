using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;

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

    public ILuaType? GetRetType(int index)
    {
        return index < rets.Count ? rets[index] : null;
    }

    public override string ToDisplayString(SearchContext context)
    {
        return string.Join(", ", rets.Select(it => it.ToDisplayString(context)));
    }
}
