using LuaLanguageServer.CodeAnalysis.Compilation.Infer;


namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public abstract class LuaType : ILuaType
{
    public LuaType(TypeKind kind)
    {
        Kind = kind;
    }

    public abstract IEnumerable<LuaTypeMember> GetMembers(SearchContext context);

    public virtual IEnumerable<LuaTypeMember> IndexMember(IndexKey key, SearchContext context) =>
        GetMembers(context).Where(it => it.MatchKey(key, context));

    public TypeKind Kind { get; }
}
