namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public interface ILuaNamedTypeSymbol : ILuaTypeSymbol
{
    public IEnumerable<string> MemberNames { get; }

    public string Name { get; }

    public string DisplayName { get; }
}
