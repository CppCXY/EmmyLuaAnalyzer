using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl.Primitive;

public class VoidSymbol : LuaSymbol
{
    public VoidSymbol() : base("void", SymbolKind.Void)
    {
    }

    public override bool SubTypeOf(ILuaSymbol symbol, SearchContext context)
        => false;
}
