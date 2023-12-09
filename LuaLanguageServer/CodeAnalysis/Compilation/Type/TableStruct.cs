using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class TableStruct(LuaDocTableTypeSyntax table) : LuaType(TypeKind.Table)
{
    public LuaDocTableTypeSyntax Table { get; } = table;

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context)
    {
        return context.FindMembers(this);
    }
}
