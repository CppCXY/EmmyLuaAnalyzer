using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Kind;

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

public class TypeOperator(TypeOperatorKind kind, LuaDeclaration luaDeclaration)
{
    public TypeOperatorKind Kind { get; } = kind;

    public LuaDeclaration LuaDeclaration { get; } = luaDeclaration;

    public virtual TypeOperator Instantiate(Dictionary<string, LuaType> genericMap) =>
        new TypeOperator(Kind, LuaDeclaration.Instantiate(genericMap));

    public virtual string BelongTypeName => string.Empty;
}

public class BinaryOperator(
    TypeOperatorKind kind,
    LuaType left,
    LuaType right,
    LuaType ret,
    LuaDeclaration luaDeclaration)
    : TypeOperator(kind, luaDeclaration)
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

    public override TypeOperator Instantiate(Dictionary<string, LuaType> genericMap) =>
        new BinaryOperator(Kind, Left.Instantiate(genericMap), Right.Instantiate(genericMap),
            Ret.Instantiate(genericMap), LuaDeclaration.Instantiate(genericMap));

    public override string BelongTypeName
    {
        get
        {
            if (Left is LuaNamedType namedType)
            {
                return namedType.Name;
            }

            return string.Empty;
        }
    }
}

public class UnaryOperator(TypeOperatorKind kind, LuaType operand, LuaType ret, LuaDeclaration luaDeclaration)
    : TypeOperator(kind, luaDeclaration)
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

    public override TypeOperator Instantiate(Dictionary<string, LuaType> genericMap) =>
        new UnaryOperator(Kind, Operand.Instantiate(genericMap), Ret.Instantiate(genericMap),
            LuaDeclaration.Instantiate(genericMap));

    public override string BelongTypeName
    {
        get
        {
            if (Operand is LuaNamedType namedType)
            {
                return namedType.Name;
            }

            return string.Empty;
        }
    }
}

public class IndexOperator(LuaType type, LuaType key, LuaType ret, LuaDeclaration luaDeclaration)
    : TypeOperator(TypeOperatorKind.Index, luaDeclaration)
{
    public LuaType Type { get; } = type;
    public LuaType Key { get; } = key;
    public LuaType Ret { get; } = ret;

    public override TypeOperator Instantiate(Dictionary<string, LuaType> genericMap) =>
        new IndexOperator(Type.Instantiate(genericMap), Key.Instantiate(genericMap),
            Ret.Instantiate(genericMap), LuaDeclaration.Instantiate(genericMap));

    public override string BelongTypeName
    {
        get
        {
            if (Type is LuaNamedType namedType)
            {
                return namedType.Name;
            }

            return string.Empty;
        }
    }
}
