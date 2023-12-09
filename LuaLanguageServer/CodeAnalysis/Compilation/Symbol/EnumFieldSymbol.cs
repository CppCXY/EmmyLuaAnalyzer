using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class EnumFieldSymbol(LuaSyntaxElement element, string name, LuaEnum containingType)
    : LuaSymbol(SymbolKind.EnumFieldSymbol, containingType)
{
    public string Name { get; } = name;

    public new LuaEnum ContainingType { get; } = containingType;

    public LuaSyntaxElement Element { get; } = element;

    public override ILuaType GetType(SearchContext context)
    {
        return ContainingType.GetBaseType(context);
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        yield return Element.Location;
    }
}
