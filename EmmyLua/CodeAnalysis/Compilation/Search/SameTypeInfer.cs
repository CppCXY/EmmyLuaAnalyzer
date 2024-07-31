using EmmyLua.CodeAnalysis.Type;
using EmmyLua.CodeAnalysis.Type.Manager;

namespace EmmyLua.CodeAnalysis.Compilation.Search;

public class SameTypeInfer(SearchContext context)
{
    private LuaTypeManager TypeManager => context.Compilation.TypeManager;

    public bool IsSameType(LuaType? left, LuaType? right)
    {
        if (left is null || right is null)
        {
            return false;
        }

        return InnerSameTypeOf(left, right);
    }

    private bool InnerSameTypeOf(LuaType left, LuaType right)
    {
        switch ((left, right))
        {
            case (LuaNamedType leftNamedType, LuaNamedType rightNamedType):
                return IsSameTypeOfNamedType(leftNamedType, rightNamedType);
            case (LuaArrayType leftArrayType, LuaArrayType rightArrayType):
                return IsSameType(leftArrayType.BaseType, rightArrayType.BaseType);
            default:
                return ReferenceEquals(left, right);
        }

        return false;
    }

    private bool IsSameTypeOfNamedType(LuaNamedType left, LuaNamedType right)
    {
        return context.Compilation.TypeManager.IsSameType(left, right);
    }
}
