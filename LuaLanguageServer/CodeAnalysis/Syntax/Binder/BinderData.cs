using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Binder;

public class BinderData
{
    private readonly Dictionary<LuaCommentSyntax, LuaSyntaxNodeOrToken> _commentOwners;
    private readonly Dictionary<LuaSyntaxNodeOrToken, List<LuaCommentSyntax>> _comments;

    public BinderData(Dictionary<LuaCommentSyntax, LuaSyntaxNodeOrToken> commentOwners,
        Dictionary<LuaSyntaxNodeOrToken, List<LuaCommentSyntax>> comments)
    {
        _commentOwners = commentOwners;
        _comments = comments;
    }

    public LuaSyntaxNodeOrToken? CommentOwner(LuaCommentSyntax comment)
    {
        return _commentOwners.TryGetValue(comment, out var owner) ? owner : null;
    }

    public IEnumerable<LuaCommentSyntax> GetComments(LuaSyntaxNodeOrToken nodeOrToken)
    {
        return _comments.TryGetValue(nodeOrToken, out var comments) ? comments : Enumerable.Empty<LuaCommentSyntax>();
    }
}
