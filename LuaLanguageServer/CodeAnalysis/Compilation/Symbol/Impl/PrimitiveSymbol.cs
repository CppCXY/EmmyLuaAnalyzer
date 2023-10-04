namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;

public class PrimitiveSymbol : LuaSymbol
{
    public PrimitiveSymbol(string name, PrimitiveTypeKind kind) : base(SymbolKind.Primitive)
    {
        Name = name;
        TypeKind = kind;
    }

    public override string Name { get; }

    public PrimitiveTypeKind TypeKind { get; }
}
