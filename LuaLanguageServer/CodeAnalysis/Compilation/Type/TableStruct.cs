using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class TableStruct : LuaType
{
    public LuaDocTableTypeSyntax Table { get; }

    public TableStruct(LuaDocTableTypeSyntax table) : base(TypeKind.Table)
    {
        Table = table;
    }

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context)
    {
        return context.FindMembers(this);
    }
}
