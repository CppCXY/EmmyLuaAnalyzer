using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Syntax.Tree.Binder;

public class BinderData(
    Dictionary<SyntaxElementId, LuaPtr<LuaSyntaxElement>> commentOwners,
    Dictionary<SyntaxElementId, List<LuaPtr<LuaCommentSyntax>>> comments)
{
    public LuaSyntaxElement? CommentOwner(LuaCommentSyntax comment)
    {
        return commentOwners.TryGetValue(comment.UniqueId, out var ptr) ? ptr.ToNode(comment.Tree.Document) : null;
    }

    public IEnumerable<LuaCommentSyntax> GetComments(LuaSyntaxElement nodeOrToken)
    {
        if (!comments.TryGetValue(nodeOrToken.UniqueId, out var list))
        {
            yield break;
        }

        foreach (var ptr in list)
        {
            if (ptr.ToNode(nodeOrToken.Tree.Document) is { } commentSyntax)
            {
                yield return commentSyntax;
            }
        }
    }
}
