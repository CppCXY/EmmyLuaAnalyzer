using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;

public class NilSymbol : LuaSymbol
{
    public NilSymbol() : base(SymbolKind.Nil)
    {
    }

    public override string Name => "nil";

    public override bool SubTypeOf(ILuaSymbol symbol, SearchContext context)
        => base.SubTypeOf(symbol, context) || symbol.Kind == SymbolKind.Nil;
}
