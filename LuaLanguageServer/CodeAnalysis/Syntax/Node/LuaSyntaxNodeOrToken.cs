using System.Diagnostics;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node;

public abstract record LuaSyntaxNodeOrToken
{
    public record Node(LuaSyntaxNode SyntaxNode) : LuaSyntaxNodeOrToken;

    public record Token(LuaSyntaxToken SyntaxToken) : LuaSyntaxNodeOrToken;

    public override int GetHashCode()
    {
        return this switch
        {
            Node { SyntaxNode: { } node } => node.GetHashCode(),
            Token { SyntaxToken: { } token } => token.GetHashCode(),
            _ => throw new UnreachableException()
        };
    }

    public virtual bool Equals(LuaSyntaxNodeOrToken? other)
    {
        return (this, other) switch
        {
            (Node { SyntaxNode: { } node }, Node { SyntaxNode: { } node2 }) => node.Equals(node2),
            (Token { SyntaxToken: { } token }, Token { SyntaxToken: { } token2 }) => token.Equals(token2),
            _ => false
        };
    }
}
