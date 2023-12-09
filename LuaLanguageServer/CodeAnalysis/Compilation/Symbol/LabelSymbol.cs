using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class LabelSymbol(LuaSyntaxElement element, string name) : LuaSymbol(SymbolKind.LabelSymbol, null)
{
    public string Name { get; } = name;

    public LuaSyntaxElement Element { get; } = element;

    public override ILuaType GetType(SearchContext context)
    {
        return context.Compilation.Builtin.Unknown;
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        yield return Element.Location;
    }
}
