using EmmyLua.CodeAnalysis.Compilation.Type;

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

    record struct SubTypeKey(LuaType Left, LuaType Right);

    public bool IsSubTypeOf(LuaType left, LuaType right)
    {
        var key = new SubTypeKey(left, right);
        if (SubTypeCaches.TryGetValue(key, out var result))
        {
            return result == SubTypeResult.True;
        }

        SubTypeCaches[key] = SubTypeResult.NoAnswer;
        var result2 = InnerSubTypeOf(left, right);
        SubTypeCaches[key] = result2 ? SubTypeResult.True : SubTypeResult.False;
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
            case (LuaAggregateType leftAggregateType, LuaAggregateType rightAggregateType):
                return IsSubTypeOfAggregateType(leftAggregateType, rightAggregateType);
            case (LuaTupleType leftTupleType, LuaTupleType rightTupleType):
                return IsSubTypeOfTupleType(leftTupleType, rightTupleType);
            case (LuaArrayType leftArrayType, LuaArrayType rightArrayType):
                return IsSubTypeOf(leftArrayType.BaseType, rightArrayType.BaseType);
        }

        return false;
    }

    private bool IsSubTypeOfNamedType(LuaNamedType left, LuaNamedType right)
    {
        if (left.Equals(right))
        {
            return true;
        }

        var supers = context.Compilation.Db.QuerySupers(left.Name);
        return supers.Any(super => IsSubTypeOf(super, right));
    }

    private bool IsSubTypeOfUnionType(LuaUnionType left, LuaUnionType right)
    {
        return left.UnionTypes.All(leftType => right.UnionTypes.Any(rightType => IsSubTypeOf(leftType, rightType)));
    }

    private bool IsSubTypeOfAggregateType(LuaAggregateType left, LuaAggregateType right)
    {
        var count = Math.Min(left.Declarations.Count, right.Declarations.Count);
        for (var i = 0; i < count; ++i)
        {
            if (!IsSubTypeOf(left.Declarations[i].Type, right.Declarations[i].Type))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsSubTypeOfTupleType(LuaTupleType left, LuaTupleType right)
    {
        if (left.TupleDeclaration.Count != right.TupleDeclaration.Count)
        {
            return false;
        }

        return !left.TupleDeclaration.Where((t, i) =>
            !t.Type.SubTypeOf(right.TupleDeclaration[i].Type, context)).Any();
    }
}
