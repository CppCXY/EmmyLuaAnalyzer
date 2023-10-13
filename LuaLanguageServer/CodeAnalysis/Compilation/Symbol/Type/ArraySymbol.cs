namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;

public class ArraySymbol : LuaSymbol
{
    public ILuaSymbol BaseSymbol { get; }

    public ArraySymbol(ILuaSymbol baseSymbol) : base(SymbolKind.Array)
    {
        BaseSymbol = baseSymbol;
    }
}
