namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Type;

public class TupleSymbol : TypeSymbol
{
    private readonly List<ILuaSymbol> _symbols = new();

    public TupleSymbol(IEnumerable<ILuaSymbol> symbols) : base(TypeKind.Tuple)
    {
        _symbols.AddRange(symbols);
    }

    public IEnumerable<ILuaSymbol> Members => _symbols;

    public ILuaSymbol? Get(int index) => index < _symbols.Count ? _symbols[index] : null;
}
