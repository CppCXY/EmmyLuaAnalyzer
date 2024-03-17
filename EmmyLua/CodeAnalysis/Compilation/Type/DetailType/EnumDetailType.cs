namespace EmmyLua.CodeAnalysis.Compilation.Type.DetailType;

public class EnumDetailType(string name, LuaType? baseType) : BasicDetailType(name, NamedTypeKind.Enum)
{
    public LuaType? BaseType { get; } = baseType;
}
