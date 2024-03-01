namespace EmmyLua.CodeAnalysis.Compilation.Type;

public enum TypeKind
{
    Unknown,
    NamedType,
    Nil,
    Alias,
    Tuple,
    Union,
    Array,
    Generic,
    Return,
    Method,
    StringLiteral,
    IntegerLiteral,
    TableLiteral
}
