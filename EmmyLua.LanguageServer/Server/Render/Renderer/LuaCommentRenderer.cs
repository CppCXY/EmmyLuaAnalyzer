using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.LanguageServer.Server.Render.Renderer;

public static class LuaCommentRenderer
{
    private static void RenderCommentDescription(IEnumerable<LuaCommentSyntax>? comments,
        LuaRenderContext renderContext)
    {
        if (comments is null)
        {
            return;
        }

        foreach (var comment in comments)
        {
            // renderContext.AddSeparator();
            renderContext.AppendLine();
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
                // renderContext.AddSeparator();
                renderContext.AppendLine();
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
            // renderContext.AddSeparator();
            renderContext.AppendLine();
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
                // renderContext.AddSeparator();
                renderContext.AppendLine();
                renderContext.Append(comment.CommentText);
            }
        }
    }

    public static void RenderEnumFieldComment(EnumFieldInfo enumFieldInfo, LuaRenderContext renderContext)
    {
        var enumFieldSyntax = enumFieldInfo.EnumFieldDefPtr.ToNode(renderContext.SearchContext);
        if (enumFieldSyntax is { Description: { CommentText: { } commentText } })
        {
            // renderContext.AddSeparator();
            renderContext.AppendLine();
            renderContext.Append(commentText);
        }
    }

    public static void RenderTypeComment(NamedTypeInfo namedTypeInfo, LuaRenderContext renderContext)
    {
        var typeDefine = namedTypeInfo.TypeDefinePtr.ToNode(renderContext.SearchContext);
        if (typeDefine is { Description: { CommentText: { } commentText } })
        {
            // renderContext.AddSeparator();
            renderContext.AppendLine();
            renderContext.Append(commentText);
        }
    }

    public static void RenderFunctionDocComment(MethodInfo methodInfo, LuaRenderContext renderContext)
    {
        var funcStat = methodInfo.Ptr.ToNode(renderContext.SearchContext)?
            .AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault();
        if (funcStat is { Comments: { } comments })
        {
            var tagParams = comments
                .SelectMany(it => it.DocList)
                .OfType<LuaDocTagParamSyntax>()
                .ToList();
            if (tagParams.Count == 0)
            {
                return;
            }

            var tagRenderList = new List<string>();
            foreach (var tagParam in tagParams)
            {
                if (tagParam.Description is { CommentText: {} commentText })
                {
                    // var detailList = details.ToList();
                    // if (detailList.Count == 0)
                    // {
                    //     continue;
                    // }
                    
                    var renderString = new StringBuilder();
                    // var nameLength = 0;
                    if (tagParam.Name is { RepresentText: { } name })
                    {
                        renderString.Append($"@param `{name}`");
                        // nameLength = name.Length;
                    }
                    else if (tagParam.VarArgs is not null)
                    {
                        renderString.Append("@param `...`");
                        // nameLength = 3;
                    }
                    
                    // var detailIndent = " - ";
                    renderString.Append($" - {commentText}");
                    // for (var index = 0; index < detailList.Count; index++)
                    // {
                    //     var detail = detailList[index];
                    //     renderString.Append($"{detailIndent}{detail.RepresentText}");
                    //     if (index < detailList.Count - 1)
                    //     {
                    //         renderString.Append("\n\n");
                    //     }
                    //
                    //     if (index == 0 && detailList.Count > 1)
                    //     {
                    //         detailIndent = new string(' ', 7 + nameLength + 3); // 8 spaces + nameLength + 3 spaces
                    //     }
                    // }
                    
                    tagRenderList.Add(renderString.ToString());
                }
                
            }
            
            if (tagRenderList.Count > 0)
            {
                renderContext.AppendLine();
                foreach (var tagRender in tagRenderList)
                {
                    renderContext.Append(tagRender);
                    renderContext.Append("\n\n");
                }
            }
        }
    }
}