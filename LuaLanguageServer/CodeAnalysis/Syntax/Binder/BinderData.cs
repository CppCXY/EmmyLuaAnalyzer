using System.Collections;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Binder;

public class BinderData
{
    private readonly Dictionary<LuaCommentSyntax, LuaSyntaxElement> _commentOwners;
    private readonly Dictionary<LuaSyntaxElement, List<LuaCommentSyntax>> _comments;
    private readonly Dictionary<LuaSyntaxElement, List<LuaSyntaxToken>> _docDescriptions;

    public BinderData(Dictionary<LuaCommentSyntax, LuaSyntaxElement> commentOwners,
        Dictionary<LuaSyntaxElement, List<LuaCommentSyntax>> comments,
        Dictionary<LuaSyntaxElement, List<LuaSyntaxToken>> docDescriptions
        )
    {
        _commentOwners = commentOwners;
        _comments = comments;
        _docDescriptions = docDescriptions;
    }

    public LuaSyntaxElement? CommentOwner(LuaCommentSyntax comment)
    {
        return _commentOwners.TryGetValue(comment, out var owner) ? owner : null;
    }

    public IEnumerable<LuaCommentSyntax> GetComments(LuaSyntaxElement nodeOrToken)
    {
        return _comments.TryGetValue(nodeOrToken, out var comments) ? comments : Enumerable.Empty<LuaCommentSyntax>();
    }

    public IEnumerable<LuaSyntaxToken> GetDescriptions(LuaSyntaxElement nodeOrToken)
    {
        return GetComments(nodeOrToken).SelectMany(it => it.Descriptions);
    }
}
