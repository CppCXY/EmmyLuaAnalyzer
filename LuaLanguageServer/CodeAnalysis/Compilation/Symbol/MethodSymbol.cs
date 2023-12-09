using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class MethodSymbol(LuaSyntaxElement element, LuaMethod method, ILuaType containingType)
    : LuaSymbol(SymbolKind.MethodSymbol, containingType)
{
    public LuaMethod Method { get; } = method;

    public LuaSyntaxElement Element { get; } = element;

    public override ILuaType GetType(SearchContext context)
    {
        return Method;
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        yield return Element.Location;
    }
}
