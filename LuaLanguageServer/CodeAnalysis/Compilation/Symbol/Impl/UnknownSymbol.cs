using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;

public class UnknownSymbol : LuaSymbol
{
    public UnknownSymbol() : base("unknown", SymbolKind.Unknown)
    {
    }

    public override bool SubTypeOf(ILuaSymbol symbol, SearchContext context) => false;
}
