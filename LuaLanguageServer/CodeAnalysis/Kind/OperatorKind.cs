namespace LuaLanguageServer.CodeAnalysis.Kind;

public static class OperatorKind
{
    public enum UnaryOperator
    {
        // Unary operators
        OpNot, // not
        OpLen, // #
        OpUnm, // -
        OpBNot, // ~
        OpNop, // (empty)
    }

    public static UnaryOperator ToUnaryOperator(LuaTokenKind kind)
    {
        return kind switch
        {
            LuaTokenKind.TkNot => UnaryOperator.OpNot,
            LuaTokenKind.TkLen => UnaryOperator.OpLen,
            LuaTokenKind.TkMinus => UnaryOperator.OpUnm,
            LuaTokenKind.TkBitXor => UnaryOperator.OpBNot,
            _ => UnaryOperator.OpNop
        };
    }

    public enum BinaryOperator
    {
        // Binary operators
        OpAdd, // +
        OpSub, // -

        OpMul, // *
        OpDiv, // /
        OpIDiv, // //
        OpMod, // %
        OpPow, // ^

        OpBAnd, // &
        OpBOr, // |
        OpBXor, // ~
        OpShl, // <<
        OpShr, // >>

        OpConcat, // ..

        OpLt, // <
        OpLe, // <=
        OpGt, // >
        OpGe, // >=
        OpEq, // ==
        OpNe, // ~=

        OpAnd, // and
        OpOr, // or

        OpNop, // (empty)
    }

    public static BinaryOperator ToBinaryOperator(LuaTokenKind kind)
    {
        return kind switch
        {
            LuaTokenKind.TkPlus => BinaryOperator.OpAdd,
            LuaTokenKind.TkMinus => BinaryOperator.OpSub,
            LuaTokenKind.TkMul => BinaryOperator.OpMul,
            LuaTokenKind.TkMod => BinaryOperator.OpMod,
            LuaTokenKind.TkPow => BinaryOperator.OpPow,
            LuaTokenKind.TkDiv => BinaryOperator.OpDiv,
            LuaTokenKind.TkIDiv => BinaryOperator.OpIDiv,
            LuaTokenKind.TkBitAnd => BinaryOperator.OpBAnd,
            LuaTokenKind.TkBitOr => BinaryOperator.OpBOr,
            LuaTokenKind.TkBitXor => BinaryOperator.OpBXor,
            LuaTokenKind.TkShl => BinaryOperator.OpShl,
            LuaTokenKind.TkShr => BinaryOperator.OpShr,
            LuaTokenKind.TkConcat => BinaryOperator.OpConcat,
            LuaTokenKind.TkLt => BinaryOperator.OpLt,
            LuaTokenKind.TkLe => BinaryOperator.OpLe,
            LuaTokenKind.TkGt => BinaryOperator.OpGt,
            LuaTokenKind.TkGe => BinaryOperator.OpGe,
            LuaTokenKind.TkEq => BinaryOperator.OpEq,
            LuaTokenKind.TkNe => BinaryOperator.OpNe,
            LuaTokenKind.TkAnd => BinaryOperator.OpAnd,
            LuaTokenKind.TkOr => BinaryOperator.OpOr,
            _ => BinaryOperator.OpNop
        };
    }

    /*
     ** Priority table for binary operators.
     */
    public struct PriorityTable
    {
        public int Left;
        public int Right;
    };

    public static readonly PriorityTable[] Priority = new PriorityTable[]
    {
        /* ORDER OPR */
        new PriorityTable { Left = 10, Right = 10 }, /* OPR_ADD */
        new PriorityTable { Left = 10, Right = 10 }, /* OPR_SUB */

        new PriorityTable { Left = 11, Right = 11 }, /* OPR_MUL */
        new PriorityTable { Left = 11, Right = 11 }, /* OPR_DIV */
        new PriorityTable { Left = 11, Right = 11 }, /* OPR_IDIV */
        new PriorityTable { Left = 11, Right = 11 }, /* OPR_MOD */
        new PriorityTable { Left = 14, Right = 13 }, /* OPR_POW */

        new PriorityTable { Left = 6, Right = 6 }, /* OPR_BAND */
        new PriorityTable { Left = 4, Right = 4 }, /* OPR_BOR */
        new PriorityTable { Left = 5, Right = 5 }, /* OPR_BXOR */
        new PriorityTable { Left = 7, Right = 7 }, /* OPR_SHL */
        new PriorityTable { Left = 7, Right = 7 }, /* OPR_SHR */

        new PriorityTable { Left = 9, Right = 8 }, /* OPR_CONCAT */

        new PriorityTable { Left = 3, Right = 3 }, /* OPR_EQ */
        new PriorityTable { Left = 3, Right = 3 }, /* OPR_LT */
        new PriorityTable { Left = 3, Right = 3 }, /* OPR_LE */
        new PriorityTable { Left = 3, Right = 3 }, /* OPR_NE */
        new PriorityTable { Left = 3, Right = 3 }, /* OPR_GT */
        new PriorityTable { Left = 3, Right = 3 }, /* OPR_GE */

        new PriorityTable { Left = 2, Right = 2 }, /* OPR_AND */
        new PriorityTable { Left = 1, Right = 1 }, /* OPR_OR */
    };

    public static int UNARY_PRIORITY = 12; /* priority for unary operators */
}
