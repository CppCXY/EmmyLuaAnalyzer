namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;

public class TupleSymbol : LuaSymbol
{
    private readonly List<ILuaSymbol> _symbols = new();

    public TupleSymbol(IEnumerable<ILuaSymbol> symbols) : base(SymbolKind.Tuple)
    {
        _symbols.AddRange(symbols);
    }

    public override IEnumerable<ILuaSymbol> Members => _symbols;

    public ILuaSymbol? Get(int index) => index < _symbols.Count ? _symbols[index] : null;
}
