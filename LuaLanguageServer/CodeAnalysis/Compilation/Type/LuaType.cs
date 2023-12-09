using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;


namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public abstract class LuaType(TypeKind kind) : ILuaType
{
    public abstract IEnumerable<ILuaSymbol> GetMembers(SearchContext context);

    public virtual IEnumerable<ILuaSymbol> IndexMember(IndexKey key, SearchContext context) => Enumerable.Empty<LuaSymbol>();

    public virtual bool SubTypeOf(ILuaType other, SearchContext context)
    {
        return ReferenceEquals(this, other);
    }

    public virtual bool AcceptExpr(LuaExprSyntax expr, SearchContext context)
    {
        var ty = context.Infer(expr);
        return SubTypeOf(ty, context);
    }

    public TypeKind Kind { get; } = kind;
}
