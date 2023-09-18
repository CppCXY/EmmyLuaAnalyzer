using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Binder;

public static class BinderAnalysis
{
    public static BinderData Analysis(LuaSyntaxNode root)
    {
        Dictionary<LuaCommentSyntax, LuaSyntaxNodeOrToken> commentOwners = new();
        Dictionary<LuaSyntaxNodeOrToken, List<LuaCommentSyntax>> comments = new();
        Dictionary<LuaSyntaxNode, List<LuaSyntaxToken>> docDescriptions = new();

        foreach (var nodeOrToken in root.DescendantsAndSelfWithTokens)
        {
            if (nodeOrToken is LuaSyntaxNodeOrToken.Node { SyntaxNode: LuaCommentSyntax commentSyntax })
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
            else if (nodeOrToken is LuaSyntaxNodeOrToken.Token
                     {
                         SyntaxToken.Kind: LuaTokenKind.TkDocDescription
                     } token)
            {
                var description = token.SyntaxToken;


            }
        }

        return new BinderData(commentOwners, comments, docDescriptions);
    }

    // 通过向前查找, 获取注释的所有者, 会忽略空白/逗号/分号
    private static LuaSyntaxNodeOrToken? GetInlineNodeOrToken(LuaCommentSyntax commentSyntax)
    {
        for (var i = 1;; i++)
        {
            var prevSibling = commentSyntax.GetPrevSibling(i);
            switch (prevSibling)
            {
                case LuaSyntaxNodeOrToken.Token
                {
                    SyntaxToken.Kind: LuaTokenKind.TkWhitespace or LuaTokenKind.TkComma or LuaTokenKind.TkSemicolon,
                }:
                {
                    continue;
                }
                case null
                    or LuaSyntaxNodeOrToken.Token { SyntaxToken.Kind: LuaTokenKind.TkEndOfLine }
                    or LuaSyntaxNodeOrToken.Node { SyntaxNode: LuaCommentSyntax }:
                {
                    return null;
                }
                case LuaSyntaxNodeOrToken.Token { SyntaxToken.Kind: not LuaTokenKind.TkName }:
                {
                    return commentSyntax.Parent != null ? new LuaSyntaxNodeOrToken.Node(commentSyntax.Parent) : null;
                }
                default:
                    return prevSibling;
            }
        }
    }

    // 通过向后查找, 获取注释的所有者, 会忽略空白/至多一个行尾
    private static LuaSyntaxNodeOrToken? GetAttachedNodeOrToken(LuaCommentSyntax commentSyntax)
    {
        var meetEndOfLine = false;
        for (var i = 1;; i++)
        {
            var nextSibling = commentSyntax.GetNextSibling(i);
            switch (nextSibling)
            {
                case LuaSyntaxNodeOrToken.Token { SyntaxToken.Kind: LuaTokenKind.TkWhitespace }:
                {
                    continue;
                }
                case LuaSyntaxNodeOrToken.Token { SyntaxToken.Kind: LuaTokenKind.TkEndOfLine }:
                {
                    if (meetEndOfLine) return null;
                    meetEndOfLine = true;
                    continue;
                }
                case null or LuaSyntaxNodeOrToken.Node { SyntaxNode: LuaCommentSyntax }:
                {
                    return null;
                }
                default:
                    return nextSibling;
            }
        }
    }

    public static LuaSyntaxNodeOrToken? GetInlineDocNode(LuaSyntaxToken descriptionToken)
    {
        for (var i = 1;; i++)
        {
            var prevSibling = descriptionToken.GetPrevSibling(i);
            switch (prevSibling)
            {
                case LuaSyntaxNodeOrToken.Token
                {
                    SyntaxToken.Kind: LuaTokenKind.TkWhitespace or LuaTokenKind.TkDocContinue,
                }:
                {
                    continue;
                }
                case null
                    or LuaSyntaxNodeOrToken.Token
                    {
                        SyntaxToken.Kind: LuaTokenKind.TkEndOfLine or LuaTokenKind.TkNormalStart
                    }:
                {
                    return null;
                }
                default:
                    return prevSibling;
            }
        }
    }
}
