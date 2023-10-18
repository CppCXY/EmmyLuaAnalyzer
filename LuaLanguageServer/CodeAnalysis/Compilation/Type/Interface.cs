namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Interface : LuaType, ILuaNamedType
{
    public IEnumerable<string> MemberNames { get; }

    public string Name { get; }

    public string DisplayName { get; }

    public Interface(string name) : base(TypeKind.Interface)
    {
    }
}
