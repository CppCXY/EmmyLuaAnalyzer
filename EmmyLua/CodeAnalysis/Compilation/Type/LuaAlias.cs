using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaAlias(string name, ILuaType baseType) : LuaType(TypeKind.Alias), ILuaNamedType
{
    public string Name { get; } = name;

    public ILuaType BaseType { get; } = baseType;

    protected override bool OnSubTypeOf(ILuaType other, SearchContext context)
    {
        return BaseType.SubTypeOf(other, context);
    }

    protected override ILuaType OnSubstitute(SearchContext context)
    {
        return BaseType.Substitute(context);
    }

    public override string ToDisplayString(SearchContext context)
    {
        return Name;
    }
}
