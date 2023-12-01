using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public class IteratorParamSymbol : LuaSymbol
{
    public string Name { get; }

    public LuaSyntaxElement Element { get; }

    public IteratorGenerator Generator { get; }

    public int ItPosition { get; }

    public IteratorParamSymbol(LuaSyntaxElement element, string name, IteratorGenerator generator,
        int itPosition = 0)
        : base(SymbolKind.LocalSymbol, null)
    {
        Name = name;
        Element = element;
        Generator = generator;
        ItPosition = itPosition;
    }

    public override ILuaType GetType(SearchContext context)
    {
        return Generator.GetType(ItPosition, context);
    }

    public override IEnumerable<LuaLocation> GetLocations(SearchContext context)
    {
        yield return Element.Location;
    }
}

public class IteratorGenerator
{
    public List<LuaExprSyntax> ExprList { get; }

    public IteratorGenerator(IEnumerable<LuaExprSyntax> exprList)
    {
        ExprList = exprList.ToList();
    }

    public ILuaType GetType(int position, SearchContext context)
    {
        throw new NotImplementedException();
    }
}
