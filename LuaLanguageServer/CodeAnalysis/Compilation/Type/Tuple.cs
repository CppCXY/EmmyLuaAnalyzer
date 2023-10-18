namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Tuple : LuaType
{
    private readonly List<ILuaType> _types = new();

    public Tuple(IEnumerable<ILuaType> symbols) : base(TypeKind.Tuple)
    {
        _types.AddRange(symbols);
    }

    public IEnumerable<ILuaType> Members => _types;

    public ILuaType? Get(int index) => index < _types.Count ? _types[index] : null;
}
