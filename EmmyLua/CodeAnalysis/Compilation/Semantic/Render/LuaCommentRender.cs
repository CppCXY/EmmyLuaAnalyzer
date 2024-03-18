using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Render;

public static class LuaCommentRender
{
    private static void RenderSeparator(StringBuilder sb)
    {
        if (sb.Length > 0)
        {
            sb.Append("\n___\n");
        }
    }

    public static void RenderCommentDescription(IEnumerable<LuaCommentSyntax>? comments, StringBuilder sb)
    {
        if (comments is null)
        {
            return;
        }

        foreach (var comment in comments)
        {
            RenderSeparator(sb);
            sb.Append(comment.CommentText);
        }
    }
}
