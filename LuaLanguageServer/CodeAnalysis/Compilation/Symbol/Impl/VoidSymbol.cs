using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;

public class VoidSymbol : LuaSymbol
{
    public VoidSymbol() : base(SymbolKind.Void)
    {
    }

    public override string Name => "void";

    public override bool SubTypeOf(ILuaSymbol symbol, SearchContext context)
        => false;
}
