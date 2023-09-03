using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node;

class SyntaxFactory
{
    public static LuaSourceSyntax SourceSyntax(GreenNode greenNode)
    {
        var block = BlockSyntax(greenNode.Children.First());
        return new LuaSourceSyntax(greenNode, block);
    }

    public static LuaBlockSyntax BlockSyntax(GreenNode greenNode)
    {
        var statementSyntaxList = new List<LuaStatementSyntax>();
        foreach (var child in greenNode.Children)
        {
            var statementSyntax = StatementSyntax(child);
            statementSyntaxList.Add(statementSyntax);
        }

        return new LuaBlockSyntax(greenNode, statementSyntaxList);
    }

    public static LuaStatementSyntax StatementSyntax(GreenNode greenNode)
    {
        return greenNode.SyntaxKind switch
        {
            LuaSyntaxKind.LocalStat => LocalStatementSyntax(greenNode),

            _ => throw new Exception("Unexpected SyntaxKind")
        };
    }

    public static LuaLocalStatementSyntax LocalStatementSyntax(GreenNode greenNode)
    {
        return new LuaLocalStatementSyntax(greenNode);
    }
}
