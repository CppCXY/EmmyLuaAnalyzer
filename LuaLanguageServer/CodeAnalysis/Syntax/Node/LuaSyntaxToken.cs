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
    public IEnumerable<LuaSyntaxNode> Ancestors
    {
        get
        {
            var node = Parent;
            while (node != null)
            {
                yield return node;
                node = node.Parent;
            }
        }
    }

    public ReadOnlySpan<char> Text => Tree.Source.Text.AsSpan(GreenNode.Range.StartOffset, GreenNode.Range.Length);

    public string RepresentText
    {
        get
        {
            switch (Kind)
            {
                // remove \' or \"
                case LuaTokenKind.TkString:
                {
                    var text = Text;
                    return text.Length > 2 ? text[1..^1].ToString() : text.ToString();
                }
                // skip [====[
                case LuaTokenKind.TkLongString:
                {
                    var text = Text;
                    var prefixCount = 0;
                    foreach (var t in text)
                    {
                        if ((t == '[' && prefixCount == 0) || t == '=')
                        {
                            prefixCount++;
                        }
                        else if (t == '[')
                        {
                            prefixCount++;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }

                    return text.Length > (prefixCount * 2)
                        ? text[prefixCount..^prefixCount].ToString()
                        : text.ToString();
                }
                default:
                {
                    return Text.ToString();
                }
            }
        }
    }

    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(new LuaSyntaxNodeOrToken.Token(this)) ?? Enumerable.Empty<LuaCommentSyntax>();

    public override int GetHashCode()
    {
        return GreenNode.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is LuaSyntaxToken token && GreenNode.Equals(token.GreenNode);
    }
}
