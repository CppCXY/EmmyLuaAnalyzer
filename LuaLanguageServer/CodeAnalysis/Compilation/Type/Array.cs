using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Array : LuaType
{
    public ILuaType Base { get; }

    public LuaArrayMember MemberType { get; }

    public Array(ILuaType baseTy) : base(TypeKind.Array)
    {
        Base = baseTy;
        MemberType = new LuaArrayMember(baseTy, this);
    }

    public override IEnumerable<LuaTypeMember> GetMembers(SearchContext context) => Enumerable.Empty<LuaTypeMember>();

    public override IEnumerable<LuaTypeMember> IndexMember(IndexKey key, SearchContext context)
    {
        switch (key)
        {
            case IndexKey.Integer:
            {
                yield return MemberType;
                break;
            }
            case IndexKey.Ty ty:
            {
                if (ty.Value.SubTypeOf(context.Compilation.Builtin.Number, context))
                {
                    yield return MemberType;
                }

                break;
            }
        }
    }
}

public class LuaArrayMember : LuaTypeMember
{
    private readonly ILuaType _type;

    public LuaArrayMember(ILuaType ty, ILuaType? containingType) : base(containingType)
    {
        _type = ty;
    }

    public override ILuaType GetType(SearchContext context)
    {
        return _type;
    }

    public override bool MatchKey(IndexKey key, SearchContext context)
    {
        return key switch
        {
            IndexKey.Integer => true,
            IndexKey.Ty ty => ty.Value == context.Compilation.Builtin.Integer ||
                              ty.Value == context.Compilation.Builtin.Number,
            _ => false
        };
    }
}
