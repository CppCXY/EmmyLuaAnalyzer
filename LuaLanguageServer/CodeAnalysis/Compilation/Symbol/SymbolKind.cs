namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public enum SymbolKind
{
    Unknown,
    NamedSymbol,
    FieldSymbol,
    IndexFieldSymbol,
    EnumFieldSymbol,
    MethodSymbol,
    LocalSymbol,
    LabelSymbol,
    VirtualSymbol,
    TypeDeclarationSymbol
}
