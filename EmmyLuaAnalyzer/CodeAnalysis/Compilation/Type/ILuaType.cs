using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Infer;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLuaAnalyzer.CodeAnalysis.Compilation.Type;

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
}

public interface ILuaNamedType : ILuaType
{
    public string Name { get; }
}

