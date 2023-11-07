using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public interface ILuaType
{
    public IEnumerable<LuaTypeMember> GetMembers(SearchContext context);

    public IEnumerable<LuaTypeMember> IndexMember(IndexKey key, SearchContext context);

    public bool SubTypeOf(ILuaType other, SearchContext context);

    public bool AcceptExpr(LuaExprSyntax expr, SearchContext context);

    public TypeKind Kind { get; }
}

public abstract record IndexKey
{
    public record Integer(long Value) : IndexKey;

    public record String(string Value) : IndexKey;

    public record Ty(ILuaType Value) : IndexKey;
}

public interface ILuaNamedType : ILuaType
{
    public string Name { get; }

    public LuaSyntaxElement? GetSyntaxElement(SearchContext context);

    public IEnumerable<GenericParam> GetGenericParams(SearchContext context);
}

public interface IGeneric : ILuaType
{
    public ILuaNamedType GetBaseType(SearchContext context);

    public IEnumerable<ILuaType> GetGenericArgs(SearchContext context);
}
