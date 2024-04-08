using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
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

    public static void RenderDeclarationStatComment(LuaDeclaration declaration, SearchContext context, StringBuilder sb)
    {
        var declarationElement = declaration.Ptr.ToNode(context);
        var comments =
            declarationElement?.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault()?.Comments;
        RenderCommentDescription(comments, sb);
    }

    public static void RenderStatComment(LuaStatSyntax statSyntax, StringBuilder sb)
    {
        var comments = statSyntax.Comments;
        RenderCommentDescription(comments, sb);
    }

    public static void RenderParamComment(ParameterLuaDeclaration parameterLuaDeclaration, SearchContext context,
        StringBuilder sb)
    {
        var declarationElement = parameterLuaDeclaration.Ptr.ToNode(context);
        var comments =
            declarationElement?.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault()?.Comments;
        if (comments is null)
        {
            return;
        }

        var tagParams = comments.SelectMany(it => it.DocList).OfType<LuaDocTagParamSyntax>();
        foreach (var tagParam in tagParams)
        {
            if (tagParam.Name?.RepresentText == parameterLuaDeclaration.Name && tagParam.Description != null)
            {
                RenderSeparator(sb);
                sb.Append(tagParam.Description.CommentText);
            }
        }
    }

    public static void RenderDocFieldComment(DocFieldLuaDeclaration fieldDeclaration, SearchContext context,
        StringBuilder sb)
    {
        var docField = fieldDeclaration.FieldDefPtr.ToNode(context);
        if (docField is { Description.CommentText: { } commentText })
        {
            RenderSeparator(sb);
            sb.Append(commentText);
        }
    }

    public static void RenderTableFieldComment(TableFieldLuaDeclaration tableFieldDeclaration, SearchContext context,
        StringBuilder sb)
    {
        var tableFieldSyntax = tableFieldDeclaration.TableFieldPtr.ToNode(context);
        if (tableFieldSyntax is { Comments: { } comments })
        {
            foreach (var comment in comments)
            {
                RenderSeparator(sb);
                sb.Append(comment.CommentText);
            }
        }
    }
}
