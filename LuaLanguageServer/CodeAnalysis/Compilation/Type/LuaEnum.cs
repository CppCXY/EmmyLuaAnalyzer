using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaEnum(string name, LuaSyntaxElement? typeElement) : LuaType(TypeKind.Enum), ILuaNamedType
{
    public string Name { get; } = name;

    public LuaSyntaxElement? TypeElement { get; } = typeElement;

    public IEnumerable<GenericParam> GetGenericParams(SearchContext context)
    {
        return Enumerable.Empty<GenericParam>();
    }

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context)
    {
        return context.FindMembers(this);
    }

    public ILuaType GetBaseType(SearchContext context)
    {
        return TypeElement is not null ? context.Infer(TypeElement) : context.Compilation.Builtin.Integer;
    }
}

