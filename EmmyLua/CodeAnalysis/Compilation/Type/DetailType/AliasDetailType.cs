namespace EmmyLua.CodeAnalysis.Compilation.Type.DetailType;

public class AliasDetailType(string name, LuaType? originType) : BasicDetailType(name, NamedTypeKind.Alias)
{
    public LuaType? OriginType { get; } = originType;
}
