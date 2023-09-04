using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaSourceSyntax : LuaSyntaxNode
{
    public LuaBlockSyntax BlockSyntax => FirstChild<LuaBlockSyntax>();

    public LuaSourceSyntax(GreenNode greenNode, LuaSyntaxTree tree)
        : base(greenNode, tree, null)
    {
    }
}

public class LuaBlockSyntax : LuaSyntaxNode
{
    public List<LuaStatementSyntax> StatementSyntaxList { get; }

    public LuaBlockSyntax(GreenNode greenNode, List<LuaStatementSyntax> statementSyntaxList)
        : base(greenNode)
    {
        StatementSyntaxList = statementSyntaxList;
    }
}
