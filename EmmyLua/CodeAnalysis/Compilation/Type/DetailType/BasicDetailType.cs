namespace EmmyLua.CodeAnalysis.Compilation.Type.DetailType;

public class BasicDetailType(string name, NamedTypeKind kind) : LuaNamedType(name)
{
    public NamedTypeKind Kind { get; } = kind;

    public bool IsEnum => Kind == NamedTypeKind.Enum;

    public bool IsInterface => Kind == NamedTypeKind.Interface;

    public bool IsClass => Kind == NamedTypeKind.Class;

    public bool IsAlias => Kind == NamedTypeKind.Alias;

    public virtual void DoLazyInit()
    {

    }
}
