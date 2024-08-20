using EmmyLua.CodeAnalysis.Type;
using EmmyLua.CodeAnalysis.Type.Manager.TypeInfo;
using EmmyLua.CodeAnalysis.Type.Types;

namespace EmmyLua.CodeAnalysis.Compilation.Search;

public class SubTypeInfer(SearchContext context)
{
    private Dictionary<SubTypeKey, SubTypeResult> SubTypeCaches { get; } = new();

    enum SubTypeResult
    {
        NoAnswer,
        True,
        False,
    }

    record struct SubTypeKey(TypeInfo Left, TypeInfo Right);

    public bool IsSubTypeOf(LuaType? left, LuaType? right)
    {
        if (left is null || right is null)
        {
            return false;
        }

        var result2 = InnerSubTypeOf(left, right);
        return result2;
    }

    private bool InnerSubTypeOf(LuaType left, LuaType right)
    {
        switch ((left, right))
        {
            case (LuaNamedType leftNamedType, LuaNamedType rightNamedType):
                return IsSubTypeOfNamedType(leftNamedType, rightNamedType);
            case (LuaUnionType leftUnionType, LuaUnionType rightUnionType):
                return IsSubTypeOfUnionType(leftUnionType, rightUnionType);
            // case (LuaAggregateType leftAggregateType, LuaAggregateType rightAggregateType):
            //     return IsSubTypeOfAggregateType(leftAggregateType, rightAggregateType);
            case (LuaTupleType leftTupleType, LuaTupleType rightTupleType):
                return IsSubTypeOfTupleType(leftTupleType, rightTupleType);
            case (LuaArrayType leftArrayType, LuaArrayType rightArrayType):
                return IsSubTypeOf(leftArrayType.BaseType, rightArrayType.BaseType);
        }

        return false;
    }

    private bool IsSubTypeOfNamedType(LuaNamedType left, LuaNamedType right)
    {
        if (left.IsSameType(right, context))
        {
            return true;
        }

        var typeInfo = context.Compilation.TypeManager.FindTypeInfo(left);
        if (typeInfo is null)
        {
            return false;
        }
        var typeInfoRight = context.Compilation.TypeManager.FindTypeInfo(right);
        if (typeInfoRight is null)
        {
            return false;
        }
        var key = new SubTypeKey(typeInfo, typeInfoRight);
        if (SubTypeCaches.TryGetValue(key, out var result))
        {
            return result == SubTypeResult.True;
        }

        var judgeResult = false;
        try
        {
            SubTypeCaches[key] = SubTypeResult.NoAnswer;
            switch (typeInfo.Kind)
            {
                case NamedTypeKind.Alias or NamedTypeKind.Enum:
                {
                    judgeResult = typeInfo.BaseType is not null && IsSubTypeOf(typeInfo.BaseType, right);
                    break;
                }
                case NamedTypeKind.Class or NamedTypeKind.Interface:
                {
                    if (typeInfo.BaseType is not null && IsSubTypeOf(typeInfo.BaseType, right))
                    {
                        judgeResult = true;
                        break;
                    }

                    if (typeInfo.Supers is not null)
                    {
                        judgeResult = typeInfo.Supers.Any(super => IsSubTypeOf(super, right));
                    }

                    break;
                }
            }

            return judgeResult;
        }
        finally
        {
            SubTypeCaches[key] = judgeResult ? SubTypeResult.True : SubTypeResult.False;
        }
    }

    private bool IsSubTypeOfUnionType(LuaUnionType left, LuaUnionType right)
    {
        return left.UnionTypes.All(leftType => right.UnionTypes.Any(rightType => IsSubTypeOf(leftType, rightType)));
    }

    // private bool IsSubTypeOfAggregateType(LuaAggregateType left, LuaAggregateType right)
    // {
    //     var count = Math.Min(left.Declarations.Count, right.Declarations.Count);
    //     for (var i = 0; i < count; ++i)
    //     {
    //         if (!IsSubTypeOf(left.Declarations[i].Type, right.Declarations[i].Type))
    //         {
    //             return false;
    //         }
    //     }
    //
    //     return true;
    // }

    private bool IsSubTypeOfTupleType(LuaTupleType left, LuaTupleType right)
    {
        if (left.TypeList.Count != right.TypeList.Count)
        {
            return false;
        }

        return !left.TypeList.Where((t, i) => !IsSubTypeOf(t, right.TypeList[i])).Any();
    }
}
