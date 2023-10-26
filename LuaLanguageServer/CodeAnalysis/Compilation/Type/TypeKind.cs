namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public enum TypeKind
{
    Unknown,
    Alias,
    Tuple,
    Union,
    Array,
    Class,
    Enum,
    Interface,
    Generic,
    Field,
    Table,
    Func,
    Primitive
}
