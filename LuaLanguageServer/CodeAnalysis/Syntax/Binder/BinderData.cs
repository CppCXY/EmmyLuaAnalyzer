using System.Collections;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Binder;

public class BinderData
{
    private readonly Dictionary<LuaCommentSyntax, LuaSyntaxNodeOrToken> _commentOwners;
    private readonly Dictionary<LuaSyntaxNodeOrToken, List<LuaCommentSyntax>> _comments;
    private readonly Dictionary<LuaSyntaxNode, List<LuaSyntaxToken>> _docDescriptions;

    public BinderData(Dictionary<LuaCommentSyntax, LuaSyntaxNodeOrToken> commentOwners,
        Dictionary<LuaSyntaxNodeOrToken, List<LuaCommentSyntax>> comments,
        Dictionary<LuaSyntaxNode, List<LuaSyntaxToken>> docDescriptions
        )
    {
        _commentOwners = commentOwners;
        _comments = comments;
        _docDescriptions = docDescriptions;
    }

    public LuaSyntaxNodeOrToken? CommentOwner(LuaCommentSyntax comment)
    {
        return _commentOwners.TryGetValue(comment, out var owner) ? owner : null;
    }

    public IEnumerable<LuaCommentSyntax> GetComments(LuaSyntaxNodeOrToken nodeOrToken)
    {
        return _comments.TryGetValue(nodeOrToken, out var comments) ? comments : Enumerable.Empty<LuaCommentSyntax>();
    }

    public IEnumerable<LuaSyntaxToken> GetDescriptions(LuaSyntaxNodeOrToken nodeOrToken)
    {
        return GetComments(nodeOrToken).SelectMany(it => it.Descriptions);
    }
}
