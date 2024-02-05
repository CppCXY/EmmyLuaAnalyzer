using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaEnum(string name, ILuaType luaType) : LuaType(TypeKind.Enum), ILuaNamedType
{
    public string Name { get; } = name;

    public ILuaType BaseType { get; } = luaType;

    protected override bool OnSubTypeOf(ILuaType other, SearchContext context)
    {
        return other is LuaEnum @enum && string.Equals(Name, @enum.Name, StringComparison.CurrentCulture);
    }

    public override string ToDisplayString(SearchContext context)
    {
        return Name;
    }
}
