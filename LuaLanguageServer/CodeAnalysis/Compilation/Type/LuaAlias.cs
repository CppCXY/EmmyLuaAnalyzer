using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaAlias(string name, ILuaType baseType) : LuaType(TypeKind.Alias), ILuaNamedType
{
    public string Name { get; } = name;

    public ILuaType BaseType { get; } = baseType;

    public IEnumerable<GenericParam> GetGenericParams(SearchContext context)
    {
        if (BaseType is ILuaNamedType namedType)
        {
            return namedType.GetGenericParams(context);
        }

        return Enumerable.Empty<GenericParam>();
    }

    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return context.FindMembers(BaseType);
    }
}
