using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl.Primitive;

public class NilSymbol : LuaSymbol
{
    public NilSymbol() : base("nil", SymbolKind.Nil)
    {
    }

    public override bool SubTypeOf(ILuaSymbol symbol, SearchContext context)
        => base.SubTypeOf(symbol, context) || symbol.Kind == SymbolKind.Nil;
}
