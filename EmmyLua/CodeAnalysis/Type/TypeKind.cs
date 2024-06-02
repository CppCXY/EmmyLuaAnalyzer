namespace EmmyLua.CodeAnalysis.Type;

public enum TypeKind
{
    Unknown,
    Any,
    NamedType,
    Template,
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
