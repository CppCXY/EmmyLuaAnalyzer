using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Syntax.Binder;

public class BinderData(
    Dictionary<LuaCommentSyntax, LuaSyntaxElement> commentOwners,
    Dictionary<LuaSyntaxElement, List<LuaCommentSyntax>> comments,
    Dictionary<LuaSyntaxElement, List<LuaSyntaxToken>> docDescriptions)
{
    public LuaSyntaxElement? CommentOwner(LuaCommentSyntax comment)
    {
        return commentOwners.GetValueOrDefault(comment);
    }

    public IEnumerable<LuaCommentSyntax> GetComments(LuaSyntaxElement nodeOrToken)
    {
        return comments.TryGetValue(nodeOrToken, out var value) ? value : Enumerable.Empty<LuaCommentSyntax>();
    }

    public IEnumerable<LuaSyntaxToken> GetDescriptions(LuaSyntaxElement nodeOrToken)
    {
        return GetComments(nodeOrToken).SelectMany(it => it.Descriptions);
    }
}
