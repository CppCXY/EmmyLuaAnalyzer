using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public static class TypeHelper
{
    public static void Each(LuaType type, Action<LuaType> action)
    {
        switch (type)
        {
            case LuaUnionType unionType:
            {
                foreach (var t in unionType.UnionTypes)
                {
                    action(t);
                }

                break;
            }
            default:
            {
                action(type);
                break;
            }
        }
    }

    public static LuaType Union(this LuaType left, LuaType right)
    {
        if (left.Equals(right))
        {
            return left;
        }

        if (left is LuaUnionType leftUnionType)
        {
            return UnionTypeMerge(leftUnionType, right);
        }
        else if (right is LuaUnionType rightUnionType)
        {
            return UnionTypeMerge(rightUnionType, left);
        }
        else if (left.Equals(Builtin.Unknown))
        {
            return right;
        }
        else if (right.Equals(Builtin.Unknown))
        {
            return left;
        }
        else
        {
            return new LuaUnionType(new List<LuaType> { left, right });
        }
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
