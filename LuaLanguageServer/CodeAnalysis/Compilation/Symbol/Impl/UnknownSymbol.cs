using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;

public class UnknownSymbol : LuaSymbol
{
    public UnknownSymbol() : base(SymbolKind.Unknown)
    {
    }

    public override string Name => "unknown";

    public override bool SubTypeOf(ILuaSymbol symbol, SearchContext context) => false;
}
