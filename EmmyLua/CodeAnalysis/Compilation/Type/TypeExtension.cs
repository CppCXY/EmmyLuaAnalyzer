using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public static class TypeExtension
{
    public static LuaType Union(this LuaType left, LuaType? right, SearchContext context)
    {
        if (left.IsSameType(right, context))
        {
            return left;
        }

        if (right is null)
        {
            return left;
        }

        if (left is LuaUnionType leftUnionType)
        {
            return UnionTypeMerge(leftUnionType, right, context);
        }

        if (right is LuaUnionType rightUnionType)
        {
            return UnionTypeMerge(rightUnionType, left, context);
        }

        if (left.IsSameType(Builtin.Unknown, context))
        {
            return right;
        }

        if (right.IsSameType(Builtin.Unknown, context))
        {
            return left;
        }

        return new LuaUnionType(new List<LuaType> { left, right });
    }

    public static LuaType Remove(this LuaType left, LuaType right, SearchContext context)
    {
        if (left.IsSameType(right, context))
        {
            return Builtin.Any;
        }

        if (left is LuaUnionType leftUnionType)
        {
            return UnionTypeRemove(leftUnionType, right);
        }

        return left;
    }

    private static LuaUnionType UnionTypeMerge(LuaUnionType left, LuaType right, SearchContext context)
    {
        var types = new List<LuaType>(left.TypeList);
        if (right is LuaUnionType rightUnionType)
        {
            types.AddRange(rightUnionType.TypeList);
        }
        else if (right.IsSameType(Builtin.Unknown, context))
        {
            return left;
        }
        else
        {
            types.Add(right);
        }

        return new LuaUnionType(types);
    }

    private static LuaType UnionTypeRemove(LuaUnionType left, LuaType right)
    {
        var types = new List<LuaType>(left.TypeList);
        if (right is LuaUnionType rightUnionType)
        {
            types.RemoveAll(rightUnionType.TypeList.Contains);
        }
        else
        {
            types.Remove(right);
        }

        if (types.Count == 0)
        {
            return types[0];
        }
        return new LuaUnionType(types);
    }
}
