using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node;

public class LuaSyntaxToken
{
    public LuaTokenKind Kind { get; }

    public GreenNode GreenNode { get; }

    public LuaSyntaxNode? Parent { get; }

    public LuaSyntaxToken(GreenNode greenNode)
    {
        Kind = greenNode.IsToken ? greenNode.TokenKind : LuaTokenKind.None;
        GreenNode = greenNode;
    }
}
