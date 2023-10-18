namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Class : LuaType, ILuaNamedType
{
    public Class() : base(TypeKind.Class)
    {
    }

    public IEnumerable<string> MemberNames { get; }
    public string Name { get; }
}
