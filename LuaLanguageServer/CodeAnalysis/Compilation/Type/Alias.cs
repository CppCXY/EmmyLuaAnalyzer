namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Alias : LuaType, ILuaNamedType
{
    public Alias(string name) : base(TypeKind.Alias)
    {
        Name = name;
    }

    public IEnumerable<string> MemberNames { get; }

    public string Name { get; }
}
