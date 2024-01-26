using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;

public class IndexDelayNode(
    LuaIndexExprSyntax indexExpr,
    DocumentId documentId,
    ILuaType? luaType,
    Symbol.Symbol? prev = null,
    LuaExprSyntax? expr = null,
    int retId = 0
    )
{
    public LuaIndexExprSyntax IndexExpr => indexExpr;

    public DocumentId DocumentId => documentId;

    public ILuaType? LuaType => luaType;

    public Symbol.Symbol? Prev => prev;

    public LuaExprSyntax? Expr => expr;

    public int RetId => retId;
}
