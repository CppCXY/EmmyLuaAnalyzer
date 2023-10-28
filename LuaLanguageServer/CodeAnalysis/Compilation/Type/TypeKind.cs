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
    Table,
    Func
}
