using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class MethodSymbol : LuaSymbol
{
    public LuaMethod Method { get; }

    public LuaSyntaxElement Element { get; }

    public MethodSymbol(LuaSyntaxElement element, LuaMethod method, ILuaType containingType)
        : base(SymbolKind.MethodSymbol, containingType)
    {
        Element = element;
        Method = method;
    }

    public override ILuaType GetType(SearchContext context)
    {
        return Method;
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        yield return Element.Location;
    }
}
