namespace EmmyLua.CodeAnalysis.Compilation.Type;

public enum TypeKind
{
    Unknown,
    Nil,
    Alias,
    Tuple,
    Union,
    Array,
    Generic,
    Method,
    StringLiteral,
    IntegerLiteral,
}
