using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Tuple : LuaType
{
    private readonly List<ILuaType> _types = new();

    public Tuple(IEnumerable<ILuaType> symbols) : base(TypeKind.Tuple)
    {
        _types.AddRange(symbols);
    }

    public override IEnumerable<TupleMember> GetMembers(SearchContext context)
    {
        return _types.Select((it, i) => new TupleMember(i, it, this));
    }
}

public class TupleMember : LuaTypeMember
{
    private long _index;

    private ILuaType _ty;

    public TupleMember(long index, ILuaType ty, ILuaType? containingType) : base(containingType)
    {
        _index = index;
        _ty = ty;
    }

    public override ILuaType GetType(SearchContext context)
    {
        return _ty;
    }

    public override bool MatchKey(IndexKey key, SearchContext context)
    {
        return key is IndexKey.Integer { Value: { } index } && index == _index;
    }
}
