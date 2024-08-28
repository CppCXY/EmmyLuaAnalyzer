namespace EmmyLua.CodeAnalysis.Compile.Kind;

public static class CompileTypeOperatorKind
{
    public enum TypeUnaryOperator
    {
        None,
        TypeOf,
        KeyOf,
    }

    public enum TypeBinaryOperator
    {
        None,
        Union,
        Intersection,
        In,
    }

    public enum TypeThreeOperator
    {
        None,
        Extends,
    }

    public static TypeUnaryOperator ToUnaryTypeOperatorKind(LuaTokenKind kind)
    {
        return kind switch
        {
            LuaTokenKind.TkDocKeyOf => TypeUnaryOperator.KeyOf,
            LuaTokenKind.TkDocTypeOf => TypeUnaryOperator.TypeOf,
            _ => TypeUnaryOperator.None
        };
    }

    public static TypeBinaryOperator ToBinaryTypeOperatorKind(LuaTokenKind kind)
    {
        return kind switch
        {
            LuaTokenKind.TkDocOr => TypeBinaryOperator.Union,
            LuaTokenKind.TkDocAnd => TypeBinaryOperator.Intersection,
            LuaTokenKind.TkDocIn => TypeBinaryOperator.In,
            _ => TypeBinaryOperator.None
        };
    }

    public static TypeThreeOperator ToThreeTypeOperatorKind(LuaTokenKind kind)
    {
        return kind switch
        {
            LuaTokenKind.TkDocExtends => TypeThreeOperator.Extends,
            _ => TypeThreeOperator.None
        };
    }

    /*
     ** Priority table for binary operators.
     */
    public struct PriorityTable(int left, int right)
    {
        public int Left = left;
        public int Right = right;
    }

    public static readonly PriorityTable[] Priority =
    [
        new PriorityTable(0, 0), // none
        new PriorityTable(1, 1), // |
        new PriorityTable(2, 2), // &
    ];
}
