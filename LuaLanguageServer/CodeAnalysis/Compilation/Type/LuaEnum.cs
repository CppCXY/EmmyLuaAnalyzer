using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaEnum : LuaType, ILuaNamedType
{
    public string Name { get; }

    public LuaSyntaxElement? TypeElement { get; }

    public IEnumerable<GenericParam> GetGenericParams(SearchContext context)
    {
        return Enumerable.Empty<GenericParam>();
    }

    public LuaEnum(string name, LuaSyntaxElement? typeElement) : base(TypeKind.Enum)
    {
        Name = name;
        TypeElement = typeElement;
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

