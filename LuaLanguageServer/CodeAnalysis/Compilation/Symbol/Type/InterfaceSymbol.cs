using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Type;

public class InterfaceSymbol : TypeSymbol, ILuaNamedTypeSymbol
{
    public IEnumerable<string> MemberNames { get; }
    public string Name { get; }
    public string DisplayName { get; }

    public InterfaceSymbol() : base(TypeKind.Interface)
    {
    }
}
