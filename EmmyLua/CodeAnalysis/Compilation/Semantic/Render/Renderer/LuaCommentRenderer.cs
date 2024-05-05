using EmmyLua.CodeAnalysis.Compilation.Declaration;
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

            renderContext.AddSeparator();
            renderContext.WrapperLanguage("plaintext", () =>
            {
                renderContext.Append("params: ");
                var indent = string.Empty;
                foreach (var tagParam in tagParams)
                {
                    var nameLength = 0;
                    if (tagParam.Name is { RepresentText: {} name })
                    {
                        renderContext.Append($"{indent}{name}");
                        nameLength = name.Length;
                    }
                    else if (tagParam.VarArgs is not null)
                    {
                        renderContext.Append($"{indent}...");
                        nameLength = 3;
                    }

                    if (indent.Length == 0)
                    {
                        indent = new string(' ', 8); // 8 spaces
                    }

                    if (tagParam.Description is { Details: {} details })
                    {
                        var detailIndent = " - ";
                        var detailList = details.ToList();
                        for (var index = 0; index < detailList.Count; index++)
                        {
                            var detail = detailList[index];
                            renderContext.Append($"{detailIndent}{detail.RepresentText}");
                            if (index < detailList.Count - 1)
                            {
                                renderContext.AppendLine();
                            }

                            if (index == 0 && detailList.Count > 1)
                            {
                                detailIndent = new string(' ', 8 + nameLength + 3); // 8 spaces + nameLength + 3 spaces
                            }
                        }
                    }
                    renderContext.AppendLine();
                }
            });
        }
    }
}
