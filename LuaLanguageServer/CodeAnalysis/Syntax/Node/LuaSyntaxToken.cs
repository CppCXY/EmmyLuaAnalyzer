using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node;

public class LuaSyntaxToken
{
    public LuaTokenKind Kind { get; }

    public GreenNode GreenNode { get; }

    public LuaSyntaxNode? Parent { get; }

    public LuaSyntaxTree Tree { get; }

    public LuaSyntaxToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
    {
        Kind = greenNode.IsToken ? greenNode.TokenKind : LuaTokenKind.None;
        GreenNode = greenNode;
        Parent = parent;
        Tree = tree;
    }

    // 遍历所有祖先
    public IEnumerable<LuaSyntaxNode> Ancestors()
    {
        var node = Parent;
        while (node != null)
        {
            yield return node;
            node = node.Parent;
        }
    }

    public ReadOnlySpan<char> Text => Tree.Source.Text.AsSpan(GreenNode.Range.StartOffset, GreenNode.Range.Length);

    public IEnumerable<LuaCommentSyntax> GetComments()
    {
        var binderData = Tree.BinderData;
        return binderData?.GetComments(new LuaSyntaxNodeOrToken.Token(this)) ?? Enumerable.Empty<LuaCommentSyntax>();
    }
}
