using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;

namespace EmmyLua.CodeAnalysis.Compilation.Search;

public class SameTypeInfer(SearchContext context)
{
    private LuaTypeManager TypeManager => context.Compilation.TypeManager;

    private Dictionary<SameTypeKey, SameTypeResult> SameTypeCaches { get; } = new();

    enum SameTypeResult
    {
        NoAnswer,
        True,
        False,
    }

    private record struct SameTypeKey(LuaTypeInfo Left, LuaTypeInfo Right);

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
            case (LuaGenericType leftGenericType, LuaGenericType rightGenericType):
                return IsSameTypeOfGenericType(leftGenericType, rightGenericType);
            case (LuaNamedType leftNamedType, LuaNamedType rightNamedType):
                return IsSameTypeOfNamedType(leftNamedType, rightNamedType);
            case (LuaArrayType leftArrayType, LuaArrayType rightArrayType):
                return IsSameType(leftArrayType.BaseType, rightArrayType.BaseType);
            default:
                return ReferenceEquals(left, right);
        }

        return false;
    }

    private bool IsSameTypeOfGenericType(LuaGenericType left, LuaGenericType right)
    {
        if (!IsSameTypeOfNamedType(left, right))
        {
            return false;
        }

        if (left.GenericArgs.Count != right.GenericArgs.Count)
        {
            return false;
        }

        for (var i = 0; i < left.GenericArgs.Count; i++)
        {
            if (!IsSameType(left.GenericArgs[i], right.GenericArgs[i]))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsSameTypeOfNamedType(LuaNamedType left, LuaNamedType right)
    {
        if (left.DocumentId == right.DocumentId && left.Name == right.Name)
        {
            return true;
        }

        var leftTypeInfo = TypeManager.FindTypeInfo(left);
        if (leftTypeInfo is null)
        {
            return false;
        }

        var rightTypeInfo = TypeManager.FindTypeInfo(right);
        if (rightTypeInfo is null)
        {
            return false;
        }

        if (leftTypeInfo == rightTypeInfo)
        {
            return true;
        }

        var key = new SameTypeKey(leftTypeInfo, rightTypeInfo);
        if (SameTypeCaches.TryGetValue(key, out var result))
        {
            return result == SameTypeResult.True;
        }

        SameTypeCaches[key] = SameTypeResult.NoAnswer;

        var sameType = false;
        if (leftTypeInfo.Kind == NamedTypeKind.Alias)
        {
            sameType = IsSameType(leftTypeInfo.BaseType, right);
        }
        else if (rightTypeInfo.Kind == NamedTypeKind.Alias)
        {
            sameType = IsSameType(left, rightTypeInfo.BaseType);
        }

        SameTypeCaches[key] = sameType ? SameTypeResult.True : SameTypeResult.False;
        return sameType;
    }
}
