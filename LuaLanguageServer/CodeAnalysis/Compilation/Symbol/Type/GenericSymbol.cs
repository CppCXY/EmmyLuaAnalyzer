namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;

public class GenericSymbol : LuaSymbol
{
    public ILuaSymbol BaseSymbol { get; }

    public GenericSymbol(ILuaSymbol baseSymbol) : base(SymbolKind.Generic)
    {
        BaseSymbol = baseSymbol;
    }

    public IEnumerable<string> GenericParams { get; }
}
