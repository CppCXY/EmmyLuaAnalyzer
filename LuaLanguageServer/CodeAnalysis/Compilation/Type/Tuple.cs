using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Tuple : LuaType
{
    private readonly List<ILuaType> _types = new();

    public Tuple(IEnumerable<ILuaType> symbols) : base(TypeKind.Tuple)
    {
        _types.AddRange(symbols);
    }

    public ILuaType? Get(int index) => index < _types.Count ? _types[index] : null;

    public override IEnumerable<ILuaType> GetMembers(SearchContext context)
    {
        return _types;
    }
}
