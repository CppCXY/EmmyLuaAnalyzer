using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaAlias : LuaType, ILuaNamedType
{
    public string Name { get; }

    public ILuaType BaseType { get; }

    public LuaAlias(string name, ILuaType baseType) : base(TypeKind.Alias)
    {
        Name = name;
        BaseType = baseType;
    }

    public IEnumerable<GenericParam> GetGenericParams(SearchContext context)
    {
        if (BaseType is ILuaNamedType namedType)
        {
            return namedType.GetGenericParams(context);
        }

        return Enumerable.Empty<GenericParam>();
    }

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context)
    {
        return context.FindMembers(BaseType);
    }
}
