using EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public interface ILuaType
{
    public IEnumerable<Declaration> GetMembers(SearchContext context);

    public IEnumerable<Declaration> IndexMember(string name, SearchContext context);

    public IEnumerable<Declaration> IndexMember(long index, SearchContext context);

    public IEnumerable<Declaration> IndexMember(ILuaType ty, SearchContext context);

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

