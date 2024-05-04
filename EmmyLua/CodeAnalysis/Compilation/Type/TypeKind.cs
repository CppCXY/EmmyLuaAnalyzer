namespace EmmyLua.CodeAnalysis.Compilation.Type;

public enum TypeKind
{
    Unknown,
    Any,
    NamedType,
    Nil,
    Tuple,
    Union,
    Aggregate,
    Array,
    Generic,
    Return,
    Method,
    StringLiteral,
    IntegerLiteral,
    TableLiteral,
    Variadic,
}
