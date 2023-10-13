using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Type;

public class EnumSymbol : TypeSymbol, ILuaNamedTypeSymbol
{
    public EnumSymbol() : base(TypeKind.Enum)
    {
    }

    public IEnumerable<string> MemberNames { get; }

    public string Name { get; }
}
