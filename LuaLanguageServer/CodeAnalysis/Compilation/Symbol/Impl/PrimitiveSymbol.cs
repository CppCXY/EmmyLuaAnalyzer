namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;

public class PrimitiveSymbol : LuaSymbol
{
    private string _name;
    public PrimitiveSymbol(string name, PrimitiveTypeKind kind) : base(SymbolKind.Primitive)
    {
        _name = name;
        TypeKind = kind;
    }

    public override string Name => _name;
    public PrimitiveTypeKind TypeKind { get; }
}
