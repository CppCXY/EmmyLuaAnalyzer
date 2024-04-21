namespace EmmyLua.CodeAnalysis.Compilation.Type;

public enum TypeKind
{
    Unknown,
    Any,
    NamedType,
    Nil,
    Tuple,
    Union,
    Array,
    Generic,
    Return,
    Method,
    StringLiteral,
    IntegerLiteral,
    TableLiteral,
    Variadic,
    Expand
}
