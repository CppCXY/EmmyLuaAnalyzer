namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;

public class AliasSymbol : LuaSymbol
{
    public AliasSymbol(string name) : base(SymbolKind.Alias)
    {
        Name = name;
    }
}
