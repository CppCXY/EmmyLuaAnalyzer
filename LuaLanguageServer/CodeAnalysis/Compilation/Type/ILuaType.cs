using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public interface ILuaType
{
    public IEnumerable<Declaration> GetMembers(SearchContext context);

    public IEnumerable<Declaration> IndexMember(string name, SearchContext context);

    public IEnumerable<Declaration> IndexMember(long index, SearchContext context);

    public IEnumerable<Declaration> IndexMember(ILuaType ty, SearchContext context);

    public bool SubTypeOf(ILuaType other, SearchContext context);

    public bool AcceptExpr(LuaExprSyntax expr, SearchContext context);

    public TypeKind Kind { get; }
}

public interface ILuaNamedType : ILuaType
{
    public string Name { get; }
}

public interface IGenericBase : ILuaNamedType
{
    public IEnumerable<Declaration> GetGenericParams(SearchContext context)
    {
        return context.FindGenericParams(Name);
    }
}

public interface IGenericImpl : ILuaType
{
    public IGenericBase BaseType { get; }

    public List<ILuaType> GenericArgs { get; }
}
