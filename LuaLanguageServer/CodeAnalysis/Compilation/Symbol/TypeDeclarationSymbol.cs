using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class TypeDeclarationSymbol(LuaSyntaxElement element, ILuaType type)
    : LuaSymbol(SymbolKind.TypeDeclarationSymbol, null)
{
    public ILuaType Type { get; } = type;

    public LuaSyntaxElement Element { get; } = element;

    public override ILuaType GetType(SearchContext context)
    {
        return Type;
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        yield return Element.Location;
    }
}
