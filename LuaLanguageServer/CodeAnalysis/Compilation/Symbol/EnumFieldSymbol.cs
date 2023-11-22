using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class EnumFieldSymbol : LuaSymbol
{
    public string Name { get; }

    public new LuaEnum ContainingType { get; }

    public LuaSyntaxElement Element { get; }

    public EnumFieldSymbol(LuaSyntaxElement element, string name, LuaEnum containingType)
        : base(SymbolKind.EnumFieldSymbol, containingType)
    {
        Element = element;
        Name = name;
        ContainingType = containingType;
    }

    public override ILuaType GetType(SearchContext context)
    {
        return ContainingType.GetBaseType(context);
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        yield return Element.Location;
    }
}
