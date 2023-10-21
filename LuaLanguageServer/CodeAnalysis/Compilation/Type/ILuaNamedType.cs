namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public interface ILuaNamedType : ILuaType
{
    public string Name { get; }
}
