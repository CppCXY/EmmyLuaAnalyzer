using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaEnum(string name, ILuaType luaType) : LuaType(TypeKind.Enum), ILuaNamedType
{
    public string Name { get; } = name;

    public ILuaType BaseType { get; } = luaType;

    public IEnumerable<GenericParam> GetGenericParams(SearchContext context)
    {
        return Enumerable.Empty<GenericParam>();
    }

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context)
    {
        return context.FindMembers(this);
    }
}

