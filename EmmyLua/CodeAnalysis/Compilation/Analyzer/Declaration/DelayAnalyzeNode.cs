using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;

public class DelayAnalyzeNode(
    LuaSyntaxNode node,
    DocumentId documentId,
    ILuaType? luaType,
    Declaration? prev,
    DeclarationScope? scope,
    LuaExprSyntax? expr,
    int retId = 0
    )
{
    public LuaSyntaxNode Node => node;

    public DocumentId DocumentId => documentId;

    public ILuaType? LuaType => luaType;

    public Declaration? Prev => prev;

    public DeclarationScope? Scope => scope;

    public LuaExprSyntax? Expr => expr;

    public int RetId => retId;
}
