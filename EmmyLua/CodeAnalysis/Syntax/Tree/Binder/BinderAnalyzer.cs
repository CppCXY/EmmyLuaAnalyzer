using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Syntax.Tree.Binder;

public static class BinderAnalyzer
{
    public static void Analyze(LuaSyntaxElement root, LuaSyntaxTree tree)
    {
        Dictionary<SyntaxElementId, LuaElementPtr<LuaSyntaxElement>> commentOwners = new();
        Dictionary<SyntaxElementId, List<LuaElementPtr<LuaCommentSyntax>>> comments = new();

        foreach (var it in root.Iter.DescendantsOfKind(LuaSyntaxKind.Comment))
        {
            var inlineIter = GetInlineIter(it);
            if (inlineIter.IsValid)
            {
                commentOwners.Add(it.UniqueId, inlineIter.ToPtr());
                if (!comments.TryGetValue(inlineIter.UniqueId, out var commentList))
                {
                    commentList = [];
                    comments.Add(inlineIter.UniqueId, commentList);
                }

                commentList.Add(it.ToPtr<LuaCommentSyntax>());
                return;
            }

            var attachedIter = GetAttachedIter(it);
            // ReSharper disable once InvertIf
            if (attachedIter.IsValid)
            {
                commentOwners.Add(it.UniqueId, attachedIter.ToPtr());
                if (!comments.TryGetValue(attachedIter.UniqueId, out var commentList))
                {
                    commentList = [];
                    comments.Add(attachedIter.UniqueId, commentList);
                }

                commentList.Add(it.ToPtr<LuaCommentSyntax>());
            }
        }

        tree.BinderData = new BinderData(commentOwners, comments);
    }

    // 通过向前查找, 获取注释的所有者, 会忽略空白/逗号/分号
    private static SyntaxIterator GetInlineIter(SyntaxIterator commentIter)
    {
        for (var i = 1;; i++)
        {
            var prevSibling = commentIter.GetPrevSibling(i);
            switch (prevSibling)
            {
                case
                {
                    TokenKind: LuaTokenKind.TkWhitespace or LuaTokenKind.TkComma or LuaTokenKind.TkSemicolon,
                }:
                {
                    continue;
                }
                case { IsValid: false }
                    or { TokenKind: LuaTokenKind.TkEndOfLine }
                    or { Kind: LuaSyntaxKind.Comment }:
                {
                    return commentIter.Default;
                }
                case { TokenKind: not LuaTokenKind.TkName }:
                {
                    return commentIter.Parent;
                }
                default:
                    return prevSibling;
            }
        }
    }

    // 通过向后查找, 获取注释的所有者, 会忽略空白/至多一个行尾
    private static SyntaxIterator GetAttachedIter(SyntaxIterator commentIter)
    {
        var meetEndOfLine = false;
        for (var i = 1;; i++)
        {
            var nextSibling = commentIter.GetNextSibling(i);
            switch (nextSibling)
            {
                case { TokenKind: LuaTokenKind.TkWhitespace }:
                {
                    continue;
                }
                case { TokenKind: LuaTokenKind.TkEndOfLine }:
                {
                    if (meetEndOfLine) return commentIter.Default;
                    meetEndOfLine = true;
                    continue;
                }
                case { IsValid: false } or { Kind: LuaSyntaxKind.Comment }:
                {
                    return commentIter.Default;
                }
                case { Kind: LuaSyntaxKind.Block }:
                {
                    return nextSibling.FirstChildNode(LuaStatSyntax.CanCast);
                }
                default:
                    return nextSibling;
            }
        }
    }
}
