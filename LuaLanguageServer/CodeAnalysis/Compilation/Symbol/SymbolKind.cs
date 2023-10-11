namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public enum SymbolKind
{
    Unknown,
    Type,
    Func,
    Table,
    Field,
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
