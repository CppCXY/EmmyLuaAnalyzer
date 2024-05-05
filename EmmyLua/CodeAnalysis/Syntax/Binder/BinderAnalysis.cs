using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Syntax.Binder;

public static class BinderAnalysis
{
    public static BinderData Analysis(LuaSyntaxElement root)
    {
        Dictionary<LuaCommentSyntax, LuaSyntaxElement> commentOwners = new();
        Dictionary<LuaSyntaxElement, List<LuaCommentSyntax>> comments = new();

        foreach (var nodeOrToken in root.DescendantsAndSelfWithTokens)
        {
            if (nodeOrToken is LuaCommentSyntax commentSyntax)
            {
                var inlineNodeOrToken = GetInlineNodeOrToken(commentSyntax);
                if (inlineNodeOrToken != null)
                {
                    commentOwners.Add(commentSyntax, inlineNodeOrToken);
                    if (!comments.TryGetValue(inlineNodeOrToken, out var commentList))
                    {
                        commentList = new List<LuaCommentSyntax>();
                        comments.Add(inlineNodeOrToken, commentList);
                    }

                    commentList.Add(commentSyntax);
                }
                else
                {
                    var attachedNodeOrToken = GetAttachedNodeOrToken(commentSyntax);
                    // ReSharper disable once InvertIf
                    if (attachedNodeOrToken != null)
                    {
                        commentOwners.Add(commentSyntax, attachedNodeOrToken);
                        if (!comments.TryGetValue(attachedNodeOrToken, out var commentList))
                        {
                            commentList = new List<LuaCommentSyntax>();
                            comments.Add(attachedNodeOrToken, commentList);
                        }

                        commentList.Add(commentSyntax);
                    }
                }
            }
        }

        return new BinderData(commentOwners, comments);
    }

    // 通过向前查找, 获取注释的所有者, 会忽略空白/逗号/分号
    private static LuaSyntaxElement? GetInlineNodeOrToken(LuaCommentSyntax commentSyntax)
    {
        for (var i = 1;; i++)
        {
            var prevSibling = commentSyntax.GetPrevSibling(i);
            switch (prevSibling)
            {
                case LuaSyntaxToken
                {
                    Kind: LuaTokenKind.TkWhitespace or LuaTokenKind.TkComma or LuaTokenKind.TkSemicolon,
                }:
                {
                    continue;
                }
                case null
                    or LuaSyntaxToken { Kind: LuaTokenKind.TkEndOfLine }
                    or LuaCommentSyntax:
                {
                    return null;
                }
                case LuaSyntaxToken { Kind: not LuaTokenKind.TkName }:
                {
                    return commentSyntax.Parent;
                }
                default:
                    return prevSibling;
            }
        }
    }

    // 通过向后查找, 获取注释的所有者, 会忽略空白/至多一个行尾
    private static LuaSyntaxElement? GetAttachedNodeOrToken(LuaCommentSyntax commentSyntax)
    {
        var meetEndOfLine = false;
        for (var i = 1;; i++)
        {
            var nextSibling = commentSyntax.GetNextSibling(i);
            switch (nextSibling)
            {
                case LuaSyntaxToken { Kind: LuaTokenKind.TkWhitespace }:
                {
                    continue;
                }
                case LuaSyntaxToken { Kind: LuaTokenKind.TkEndOfLine }:
                {
                    if (meetEndOfLine) return null;
                    meetEndOfLine = true;
                    continue;
                }
                case null or LuaCommentSyntax:
                {
                    return null;
                }
                default:
                    return nextSibling;
            }
        }
    }
}
