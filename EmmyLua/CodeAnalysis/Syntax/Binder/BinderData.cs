using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Syntax.Binder;

public class BinderData(
    Dictionary<LuaCommentSyntax, LuaSyntaxElement> commentOwners,
    Dictionary<LuaSyntaxElement, List<LuaCommentSyntax>> comments)
{
    public LuaSyntaxElement? CommentOwner(LuaCommentSyntax comment)
    {
        return commentOwners.GetValueOrDefault(comment);
    }

    public IEnumerable<LuaCommentSyntax> GetComments(LuaSyntaxElement nodeOrToken)
    {
        return comments.TryGetValue(nodeOrToken, out var value) ? value : Enumerable.Empty<LuaCommentSyntax>();
    }

    // public LuaDescriptionSyntax GetDescriptions(LuaSyntaxElement nodeOrToken)
    // {
    //     return GetComments(nodeOrToken).SelectMany(it => it.Descriptions);
    // }
}
