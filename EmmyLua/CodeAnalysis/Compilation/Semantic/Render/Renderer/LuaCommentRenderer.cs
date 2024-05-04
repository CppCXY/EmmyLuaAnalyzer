using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Render.Renderer;

internal static class LuaCommentRenderer
{
    public static void RenderCommentDescription(IEnumerable<LuaCommentSyntax>? comments, LuaRenderContext renderContext)
    {
        if (comments is null)
        {
            return;
        }

        foreach (var comment in comments)
        {
            renderContext.AddSeparator();
            renderContext.Append(comment.CommentText);
        }
    }

    public static void RenderDeclarationStatComment(LuaDeclaration declaration, LuaRenderContext renderContext)
    {
        var declarationElement = declaration.Info.Ptr.ToNode(renderContext.SearchContext);
        var comments =
            declarationElement?.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault()?.Comments;
        RenderCommentDescription(comments, renderContext);
    }

    public static void RenderStatComment(LuaStatSyntax statSyntax, LuaRenderContext renderContext)
    {
        var comments = statSyntax.Comments;
        RenderCommentDescription(comments, renderContext);
    }

    public static void RenderParamComment(LuaDeclaration paramDeclaration, LuaRenderContext renderContext)
    {
        var declarationElement = paramDeclaration.Info.Ptr.ToNode(renderContext.SearchContext);
        var comments =
            declarationElement?.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault()?.Comments;
        if (comments is null)
        {
            return;
        }

        var tagParams = comments.SelectMany(it => it.DocList).OfType<LuaDocTagParamSyntax>();
        foreach (var tagParam in tagParams)
        {
            if (tagParam.Name?.RepresentText == paramDeclaration.Name && tagParam.Description != null)
            {
                renderContext.AddSeparator();
                renderContext.Append(tagParam.Description.CommentText);
                break;
            }
        }
    }

    public static void RenderDocFieldComment(DocFieldInfo fieldInfo, LuaRenderContext renderContext)
    {
        var docField = fieldInfo.FieldDefPtr.ToNode(renderContext.SearchContext);
        if (docField is { Description.CommentText: { } commentText })
        {
            renderContext.AddSeparator();
            renderContext.Append(commentText);
        }
    }

    public static void RenderTableFieldComment(TableFieldInfo tableFieldInfo, LuaRenderContext renderContext)
    {
        var tableFieldSyntax = tableFieldInfo.TableFieldPtr.ToNode(renderContext.SearchContext);
        if (tableFieldSyntax is { Comments: { } comments })
        {
            foreach (var comment in comments)
            {
                renderContext.AddSeparator();
                renderContext.Append(comment.CommentText);
            }
        }
    }

    public static void RenderEnumFieldComment(EnumFieldInfo enumFieldInfo, LuaRenderContext renderContext)
    {
        var enumFieldSyntax = enumFieldInfo.EnumFieldDefPtr.ToNode(renderContext.SearchContext);
        if (enumFieldSyntax is { Description: { CommentText: { } commentText } })
        {
            renderContext.AddSeparator();
            renderContext.Append(commentText);
        }
    }

    public static void RenderTypeComment(NamedTypeInfo namedTypeInfo, LuaRenderContext renderContext)
    {
        var typeDefine = namedTypeInfo.TypeDefinePtr.ToNode(renderContext.SearchContext);
        if (typeDefine is { Description: { CommentText: { } commentText } })
        {
            renderContext.AddSeparator();
            renderContext.Append(commentText);
        }
    }
}
