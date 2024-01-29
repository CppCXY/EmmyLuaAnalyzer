using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public interface ILuaType
{
    public IEnumerable<Symbol.Symbol> GetMembers(SearchContext context);

    public IEnumerable<Symbol.Symbol> IndexMember(string name, SearchContext context);

    public IEnumerable<Symbol.Symbol> IndexMember(long index, SearchContext context);

    public IEnumerable<Symbol.Symbol> IndexMember(ILuaType ty, SearchContext context);

    public bool SubTypeOf(ILuaType other, SearchContext context);

    public bool AcceptExpr(LuaExprSyntax expr, SearchContext context);

    public ILuaType Substitute(SearchContext context);

    public ILuaType Substitute(SearchContext context, Dictionary<string, ILuaType> env);

    public TypeKind Kind { get; }

    public string ToDisplayString(SearchContext context);

    public bool IsNullable { get; }
}

public interface ILuaNamedType : ILuaType
{
    public string Name { get; }
}

