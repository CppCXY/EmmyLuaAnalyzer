using LuaLanguageServer.CodeAnalysis.Syntax.Green;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaSourceSyntax : LuaSyntaxNode
{
    public LuaBlockSyntax BlockSyntax { get; }

    public LuaSourceSyntax(GreenNode greenNode, LuaBlockSyntax blockSyntax)
        : base(greenNode)
    {
        BlockSyntax = blockSyntax;
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
