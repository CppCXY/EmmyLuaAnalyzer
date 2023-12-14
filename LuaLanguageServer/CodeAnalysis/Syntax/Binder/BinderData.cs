using System.Collections;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Binder;

public class BinderData(
    Dictionary<LuaCommentSyntax, LuaSyntaxElement> commentOwners,
    Dictionary<LuaSyntaxElement, List<LuaCommentSyntax>> comments,
    Dictionary<LuaSyntaxElement, List<LuaSyntaxToken>> docDescriptions)
{
    private readonly Dictionary<LuaSyntaxElement, List<LuaSyntaxToken>> _docDescriptions = docDescriptions;

    public LuaSyntaxElement? CommentOwner(LuaCommentSyntax comment)
    {
        return commentOwners.TryGetValue(comment, out var owner) ? owner : null;
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
