using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public interface ILuaType
{
    public IEnumerable<Declaration> GetMembers(SearchContext context);

    public IEnumerable<Declaration> IndexMember(string name, SearchContext context);

    public bool SubTypeOf(ILuaType other, SearchContext context);

    public bool AcceptExpr(LuaExprSyntax expr, SearchContext context);

    public TypeKind Kind { get; }
}

public interface ILuaNamedType : ILuaType
{
    public string Name { get; }

    public IEnumerable<GenericParam> GetGenericParams(SearchContext context);
}

public interface IGeneric : ILuaType
{
    public ILuaNamedType GetBaseType(SearchContext context);

    public IEnumerable<ILuaType> GetGenericArgs(SearchContext context);
}
