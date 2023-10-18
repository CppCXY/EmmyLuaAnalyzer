namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Enum : LuaType, ILuaNamedType
{
    public Enum() : base(TypeKind.Enum)
    {
    }

    public IEnumerable<string> MemberNames { get; }

    public string Name { get; }
}
