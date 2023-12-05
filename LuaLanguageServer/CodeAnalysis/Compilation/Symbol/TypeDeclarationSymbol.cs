using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class TypeDeclarationSymbol : LuaSymbol
{
    public ILuaType Type { get; }

    public LuaSyntaxElement Element { get; }

    public TypeDeclarationSymbol(LuaSyntaxElement element, ILuaType type) : base(
        SymbolKind.TypeDeclarationSymbol, null)
    {
        Element = element;
        Type = type;
    }

    public override ILuaType GetType(SearchContext context)
    {
        return Type;
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        yield return Element.Location;
    }
}
