namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Type;

public class GenericSymbol : TypeSymbol
{
    public GenericSymbol() : base(TypeKind.Generic)
    {
    }
}
