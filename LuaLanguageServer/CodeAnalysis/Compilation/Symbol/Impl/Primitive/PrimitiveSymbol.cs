namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl.Primitive;

public class PrimitiveSymbol : LuaSymbol
{
    public PrimitiveSymbol(string name, PrimitiveTypeKind kind) : base(name, SymbolKind.Primitive)
    {
        TypeKind = kind;
    }

    public PrimitiveTypeKind TypeKind { get; }
}
