namespace LuaLanguageServer.CodeAnalysis.Syntax.Node;

public class LuaSyntaxNodeOrToken
{
    public LuaSyntaxNodeOrToken(LuaSyntaxNode node)
    {
        Node = node;
    }

    public LuaSyntaxNodeOrToken(LuaSyntaxToken token)
    {
        Token = token;
    }

    public LuaSyntaxNode? Node { get; }

    public LuaSyntaxToken? Token { get; }

    public bool IsNode => Node != null;

    public bool IsToken => Token != null;
}
