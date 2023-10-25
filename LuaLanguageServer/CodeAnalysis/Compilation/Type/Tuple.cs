using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Tuple : LuaType
{
    private readonly List<ILuaType> _types = new();

    public Tuple(IEnumerable<ILuaType> symbols) : base(TypeKind.Tuple)
    {
        _types.AddRange(symbols);
    }

    public override IEnumerable<ILuaType> GetMembers(SearchContext context)
    {
        return _types;
    }

    public override IEnumerable<ILuaType> IndexMember(IndexKey key, SearchContext context)
    {
        switch (key)
        {
            case IndexKey.Integer integer:
            {
                if (integer.Value >= 1 && integer.Value <= _types.Count)
                {
                    yield return _types[(int) integer.Value - 1];
                }
                break;
            }
        }
    }
}
