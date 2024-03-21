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

    public static void RenderStatComment(LuaDeclaration declaration, StringBuilder sb)
    {
        var declarationElement = declaration.SyntaxElement;
        var comments =
            declarationElement?.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault()?.Comments;
        RenderCommentDescription(comments, sb);
    }

    public static void RenderParamComment(ParameterLuaDeclaration parameterLuaDeclaration, StringBuilder sb)
    {
        var declarationElement = parameterLuaDeclaration.SyntaxElement;
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

    public static void RenderDocFieldComment(DocFieldLuaDeclaration docField, StringBuilder sb)
    {
        // var declarationElement = docField.SyntaxElement;
        // var comments =
        //     declarationElement?.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault()?.Comments;
        // if (comments is null)
        // {
        //     return;
        // }
        //
        // var tagParams = comments.SelectMany(it => it.DocList).OfType<LuaDocTagFieldSyntax>();
        // foreach (var tagParam in tagParams)
        // {
        //     if (tagParam.Name?.RepresentText == docField.Name && tagParam.Description != null)
        //     {
        //         RenderSeparator(sb);
        //         sb.Append(tagParam.Description.CommentText);
        //     }
        // }
    }

    public static void RenderTableFieldComment(TableFieldLuaDeclaration tableField, StringBuilder sb)
    {
        var tableFieldSyntax = tableField.TableField;
        var comments = tableFieldSyntax.Comments;

        foreach (var comment in comments)
        {
            RenderSeparator(sb);
            sb.Append(comment.CommentText);
        }
    }
}
