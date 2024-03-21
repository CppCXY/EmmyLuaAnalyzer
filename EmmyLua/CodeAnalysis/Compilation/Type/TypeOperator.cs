using System.Diagnostics;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public enum TypeOperatorKind
{
    None,

    // math
    Add, // +
    Sub, // -
    Mul, // *
    Div, // /
    Mod, // %
    Pow, // ^
    Unm, // -
    Idiv, // //
    Band, // &
    Bor, // |
    Bxor, // ~
    Bnot, // ~
    Shl, // <<
    Shr, // >>

    // concat
    Concat, // ..

    // len
    Len, // #

    // compare
    Eq, // ==
    Lt, // <
    Le, // <=

    // index
    Index, // []
}

public static class TypeOperatorKindHelper
{
    public static TypeOperatorKind ToTypeOperatorKind(OperatorKind.BinaryOperator kind) => kind switch
    {
        OperatorKind.BinaryOperator.OpAdd => TypeOperatorKind.Add,
        OperatorKind.BinaryOperator.OpSub => TypeOperatorKind.Sub,
        OperatorKind.BinaryOperator.OpMul => TypeOperatorKind.Mul,
        OperatorKind.BinaryOperator.OpDiv => TypeOperatorKind.Div,
        OperatorKind.BinaryOperator.OpMod => TypeOperatorKind.Mod,
        OperatorKind.BinaryOperator.OpPow => TypeOperatorKind.Pow,
        OperatorKind.BinaryOperator.OpIDiv => TypeOperatorKind.Idiv,
        OperatorKind.BinaryOperator.OpBAnd => TypeOperatorKind.Band,
        OperatorKind.BinaryOperator.OpBOr => TypeOperatorKind.Bor,
        OperatorKind.BinaryOperator.OpBXor => TypeOperatorKind.Bxor,
        OperatorKind.BinaryOperator.OpShl => TypeOperatorKind.Shl,
        OperatorKind.BinaryOperator.OpShr => TypeOperatorKind.Shr,
        OperatorKind.BinaryOperator.OpConcat => TypeOperatorKind.Concat,
        OperatorKind.BinaryOperator.OpEq => TypeOperatorKind.Eq,
        OperatorKind.BinaryOperator.OpLt => TypeOperatorKind.Lt,
        OperatorKind.BinaryOperator.OpLe => TypeOperatorKind.Le,
        _ => TypeOperatorKind.None
    };

    public static TypeOperatorKind ToTypeOperatorKind(OperatorKind.UnaryOperator kind) => kind switch
    {
        OperatorKind.UnaryOperator.OpUnm => TypeOperatorKind.Unm,
        OperatorKind.UnaryOperator.OpBNot => TypeOperatorKind.Bnot,
        OperatorKind.UnaryOperator.OpLen => TypeOperatorKind.Len,
        _ => TypeOperatorKind.None
    };
}

public class TypeOperator(TypeOperatorKind kind, LuaSyntaxElement defineElement)
{
    public TypeOperatorKind Kind { get; } = kind;

    public LuaSyntaxElement DefineElement { get; } = defineElement;
}

public class BinaryOperator(
    TypeOperatorKind kind,
    LuaType left,
    LuaType right,
    LuaType ret,
    LuaSyntaxElement defineElement)
    : TypeOperator(kind, defineElement)
{
    public LuaType Left { get; } = left;
    public LuaType Right { get; } = right;
    public LuaType Ret { get; } = ret;

    public bool IsArithmetic => Kind switch
    {
        TypeOperatorKind.Add => true,
        TypeOperatorKind.Sub => true,
        TypeOperatorKind.Mul => true,
        TypeOperatorKind.Div => true,
        TypeOperatorKind.Mod => true,
        TypeOperatorKind.Pow => true,
        TypeOperatorKind.Unm => true,
        TypeOperatorKind.Idiv => true,
        TypeOperatorKind.Band => true,
        TypeOperatorKind.Bor => true,
        TypeOperatorKind.Bxor => true,
        TypeOperatorKind.Bnot => true,
        TypeOperatorKind.Shl => true,
        TypeOperatorKind.Shr => true,
        _ => false,
    };

    public bool IsConcat => Kind == TypeOperatorKind.Concat;

    public bool IsCompare => Kind switch
    {
        TypeOperatorKind.Eq => true,
        TypeOperatorKind.Lt => true,
        TypeOperatorKind.Le => true,
        _ => false,
    };
}

public class UnaryOperator(TypeOperatorKind kind, LuaType operand, LuaType ret, LuaSyntaxElement defineElement)
    : TypeOperator(kind, defineElement)
{
    public LuaType Operand { get; } = operand;
    public LuaType Ret { get; } = ret;

    public bool IsArithmetic => Kind switch
    {
        TypeOperatorKind.Unm => true,
        TypeOperatorKind.Bnot => true,
        _ => false,
    };

    public bool IsLen => Kind == TypeOperatorKind.Len;
}

public class IndexOperator(LuaType type, LuaType key, LuaType ret, LuaSyntaxElement defineElement)
    : TypeOperator(TypeOperatorKind.Index, defineElement)
{
    public LuaType Type { get; } = type;
    public LuaType Key { get; } = key;
    public LuaType Ret { get; } = ret;
}
