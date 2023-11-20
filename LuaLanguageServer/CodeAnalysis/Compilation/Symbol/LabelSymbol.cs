using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class LabelSymbol : LuaSymbol
{
    public string Name { get; }

    public LuaSyntaxElement Element { get; }

    public LabelSymbol(LuaSyntaxElement element, string name)
        : base(SymbolKind.LabelSymbol, null)
    {
        Name = name;
        Element = element;
    }

    public override ILuaType GetType(SearchContext context)
    {
        return context.Compilation.Builtin.Unknown;
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        yield return Element.Location;
    }
}
