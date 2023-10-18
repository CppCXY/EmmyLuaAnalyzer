namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public interface ILuaNamedType : ILuaType
{
    public IEnumerable<string> MemberNames { get; }

    public string Name { get; }
}
