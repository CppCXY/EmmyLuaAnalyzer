using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;


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

    public virtual bool SubTypeOf(ILuaType other, SearchContext context)
    {
        return ReferenceEquals(this, other);
    }

    public virtual bool AcceptExpr(LuaExprSyntax expr, SearchContext context)
    {
        var ty = context.Infer(expr);
        return SubTypeOf(ty, context);
    }

    public TypeKind Kind { get; }
}
