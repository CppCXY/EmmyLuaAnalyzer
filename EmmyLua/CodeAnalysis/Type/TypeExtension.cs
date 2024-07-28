namespace EmmyLua.CodeAnalysis.Type;

// TODO use isSame
public static class TypeExtension
{
    public static LuaType Union(this LuaType left, LuaType? right)
    {
        if (left.Equals(right))
        {
            return left;
        }

        if (right is null)
        {
            return left;
        }

        if (left is LuaUnionType leftUnionType)
        {
            return UnionTypeMerge(leftUnionType, right);
        }

        if (right is LuaUnionType rightUnionType)
        {
            return UnionTypeMerge(rightUnionType, left);
        }

        if (left.Equals(Builtin.Unknown))
        {
            return right;
        }

        if (right.Equals(Builtin.Unknown))
        {
            return left;
        }

        return new LuaUnionType(new List<LuaType> { left, right });
    }

    public static LuaType Remove(this LuaType left, LuaType right)
    {
        if (left.Equals(right))
        {
            return Builtin.Any;
        }

        if (left is LuaUnionType leftUnionType)
        {
            return UnionTypeRemove(leftUnionType, right);
        }

        return left;
    }

    private static LuaUnionType UnionTypeMerge(LuaUnionType left, LuaType right)
    {
        var types = new List<LuaType>(left.UnionTypes);
        if (right is LuaUnionType rightUnionType)
        {
            types.AddRange(rightUnionType.UnionTypes);
        }
        else if (right.Equals(Builtin.Unknown))
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
        var types = new List<LuaType>(left.UnionTypes);
        if (right is LuaUnionType rightUnionType)
        {
            types.RemoveAll(rightUnionType.UnionTypes.Contains);
        }
        else
        {
            types.Remove(right);
        }

        return new LuaUnionType(types);
    }
}
