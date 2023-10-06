namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public enum SymbolKind
{
    Unknown,
    Nil,
    Void,
    Primitive,
    Union,
    Func,
    Array,
    Alias,
    Class,
    Enum,
    Field,
    Interface,
    Label,
    Local,
    Method,
    Module,
    Parameter,
}

public enum PrimitiveTypeKind
{
    Unknown,
    Boolean,
    Integer,
    Number,
    String,
    Table,
    Void,
    Function,
}
