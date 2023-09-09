namespace LuaLanguageServer.CodeAnalysis.Syntax.Node;

public abstract record LuaSyntaxNodeOrToken
{
    public record Node(LuaSyntaxNode SyntaxNode) : LuaSyntaxNodeOrToken;

    public record Token(LuaSyntaxToken SyntaxToken) : LuaSyntaxNodeOrToken;
}
