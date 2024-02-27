namespace EmmyLua.CodeAnalysis.Compilation.Type;

public enum TypeOperatorKind
{
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

public class TypeOperator(TypeOperatorKind kind)
{
    public TypeOperatorKind Kind { get; } = kind;
}

public class BinaryOperator(TypeOperatorKind kind, LuaType left, LuaType right, LuaType ret) : TypeOperator(kind)
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

public class UnaryOperator(TypeOperatorKind kind, LuaType operand, LuaType ret) : TypeOperator(kind)
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

public class IndexOperator(LuaType type, LuaType key, LuaType ret) : TypeOperator(TypeOperatorKind.Index)
{
    public LuaType Type { get; } = type;
    public LuaType Key { get; } = key;
    public LuaType Ret { get; } = ret;
}
