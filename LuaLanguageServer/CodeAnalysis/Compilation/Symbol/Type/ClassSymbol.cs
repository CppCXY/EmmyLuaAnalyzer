namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Type;

public class ClassSymbol : TypeSymbol, ILuaNamedTypeSymbol
{
    public ClassSymbol() : base(TypeKind.Class)
    {
    }

    public IEnumerable<string> MemberNames { get; }
    public string Name { get; }
}
