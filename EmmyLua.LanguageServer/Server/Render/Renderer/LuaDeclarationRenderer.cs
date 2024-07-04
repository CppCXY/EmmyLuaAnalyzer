using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.LanguageServer.Server.Render.Renderer;

public static class LuaDeclarationRenderer
{
    public static void RenderDeclaration(LuaDeclaration declaration, LuaRenderContext renderContext)
    {
        switch (declaration.Info)
        {
            case LocalInfo localInfo:
            {
                RenderLocalDeclaration(declaration, localInfo, renderContext);
                break;
            }
            case GlobalInfo globalInfo:
            {
                RenderGlobalDeclaration(declaration, globalInfo, renderContext);
                break;
            }
            case MethodInfo methodInfo:
            {
                RenderMethodDeclaration(declaration, methodInfo, renderContext);
                break;
            }
            case ParamInfo paramInfo:
            {
                RenderParamDeclaration(declaration, paramInfo, renderContext);
                break;
            }
            case DocFieldInfo docFieldInfo:
            {
                RenderDocFieldDeclaration(declaration, docFieldInfo, renderContext);
                break;
            }
            case TableFieldInfo tableFieldInfo:
            {
                RenderTableFieldDeclaration(declaration, tableFieldInfo, renderContext);
                break;
            }
            case NamedTypeInfo namedTypeInfo:
            {
                RenderNamedTypeDeclaration(declaration, namedTypeInfo, renderContext);
                break;
            }
            case TypeIndexInfo typeIndexInfo:
            {
                RenderTypeIndexDeclaration(declaration, typeIndexInfo, renderContext);
                break;
            }
            case IndexInfo indexInfo:
            {
                RenderIndexDeclaration(declaration, indexInfo, renderContext);
                break;
            }
        }
    }

    private static void RenderInClass(LuaIndexExprSyntax indexExpr, LuaRenderContext renderContext)
    {
        var prefixType = renderContext.SearchContext.InferAndUnwrap(indexExpr.PrefixExpr);
        if (!prefixType.Equals(Builtin.Unknown))
        {
            RenderBelongType(prefixType, renderContext);
        }
    }

    private static void RenderBelongType(LuaType prefixType, LuaRenderContext renderContext)
    {
        if (!prefixType.Equals(Builtin.Unknown))
        {
            var parentTypeDescription = "class";
            if (prefixType is LuaNamedType namedType)
            {
                var namedTypeKind = namedType.GetTypeKind(renderContext.SearchContext);
                if (namedTypeKind == NamedTypeKind.Alias)
                {
                    parentTypeDescription = "alias";
                }
                else if (namedTypeKind == NamedTypeKind.Enum)
                {
                    parentTypeDescription = "enum";
                }
                else if (namedTypeKind == NamedTypeKind.Interface)
                {
                    parentTypeDescription = "interface";
                }
            }

            renderContext.Append($"\nin {parentTypeDescription} `");
            LuaTypeRenderer.RenderType(prefixType, renderContext);
            renderContext.Append("`");
        }
    }

    private static void RenderLocalDeclaration(LuaDeclaration declaration, LocalInfo localInfo,
        LuaRenderContext renderContext)
    {
        var attrib = "";
        if (localInfo.IsConst)
        {
            attrib = " <const>";
        }
        else if (localInfo.IsClose)
        {
            attrib = " <close>";
        }

        renderContext.WrapperLua(() =>
        {
            renderContext.Append($"local {declaration.Name}{attrib} : ");
            LuaTypeRenderer.RenderType(localInfo.DeclarationType, renderContext);
            LuaTypeRenderer.RenderDefinedType(localInfo.DeclarationType, renderContext);
        });

        LuaCommentRenderer.RenderDeclarationStatComment(declaration, renderContext);
    }

    private static void RenderGlobalDeclaration(LuaDeclaration declaration, GlobalInfo globalInfo,
        LuaRenderContext renderContext)
    {
        renderContext.WrapperLua(() =>
        {
            renderContext.Append($"global {declaration.Name}: ");
            LuaTypeRenderer.RenderType(globalInfo.DeclarationType, renderContext);
            LuaTypeRenderer.RenderDefinedType(globalInfo.DeclarationType, renderContext);
        });

        LuaCommentRenderer.RenderDeclarationStatComment(declaration, renderContext);
    }

    private static void RenderLiteral(LuaLiteralExprSyntax expr, LuaRenderContext renderContext)
    {
        switch (expr.Literal)
        {
            case LuaStringToken stringLiteral:
            {
                renderContext.Append($" = '{stringLiteral.Value}'");
                break;
            }
            case LuaIntegerToken integerLiteral:
            {
                renderContext.Append($" = {integerLiteral.Value}");
                break;
            }
            case LuaFloatToken floatToken:
            {
                renderContext.Append($" = {floatToken.Value}");
                break;
            }
            case LuaComplexToken complexToken:
            {
                renderContext.Append($" = {complexToken}");
                break;
            }
        }
    }

    private static void RenderMethodDeclaration(LuaDeclaration declaration, MethodInfo methodInfo,
        LuaRenderContext renderContext)
    {
        renderContext.EnableAliasRender();
        if (declaration.IsLocal)
        {
            renderContext.WrapperLua(() =>
            {
                renderContext.Append($"local function {declaration.Name}");
                LuaTypeRenderer.RenderFunc(methodInfo.Method, renderContext);
            });
        }
        else
        {
            renderContext.WrapperLua(() =>
            {
                renderContext.Append($"function {declaration.Name}");
                LuaTypeRenderer.RenderFunc(methodInfo.Method, renderContext);
            });

            if (methodInfo.IndexPtr.ToNode(renderContext.SearchContext) is { } indexExpr)
            {
                RenderInClass(indexExpr, renderContext);
            }
        }

        LuaCommentRenderer.RenderDeclarationStatComment(declaration, renderContext);
        LuaCommentRenderer.RenderFunctionDocComment(methodInfo, renderContext);
    }

    private static void RenderParamDeclaration(LuaDeclaration declaration, ParamInfo paramInfo,
        LuaRenderContext renderContext)
    {
        renderContext.EnableAliasRender();
        if (paramInfo.DeclarationType is { } declarationType)
        {
            renderContext.WrapperLua(() =>
            {
                renderContext.Append($"(parameter) {declaration.Name} : ");
                LuaTypeRenderer.RenderType(declarationType, renderContext);
            });
        }
        else
        {
            renderContext.WrapperLua(() => { renderContext.AppendLine($"(parameter) {declaration.Name}"); });
        }

        LuaCommentRenderer.RenderParamComment(declaration, renderContext);
    }

    private static void RenderDocFieldDeclaration(LuaDeclaration declaration, DocFieldInfo docFieldInfo,
        LuaRenderContext renderContext)
    {
        renderContext.WrapperLua(() =>
        {
            var visibility = declaration.Visibility switch
            {
                DeclarationVisibility.Public => "",
                DeclarationVisibility.Protected => "protected ",
                DeclarationVisibility.Private => "private ",
                _ => ""
            };
            renderContext.Append($"{visibility}(field) {declaration.Name} : ");
            LuaTypeRenderer.RenderType(docFieldInfo.DeclarationType, renderContext);
        });
        LuaCommentRenderer.RenderDocFieldComment(docFieldInfo, renderContext);
    }

    private static void RenderTableFieldDeclaration(LuaDeclaration declaration, TableFieldInfo tableFieldInfo,
        LuaRenderContext renderContext)
    {
        renderContext.WrapperLua(() =>
        {
            var visibility = declaration.Visibility switch
            {
                DeclarationVisibility.Public => "",
                DeclarationVisibility.Protected => "protected ",
                DeclarationVisibility.Private => "private ",
                _ => ""
            };

            renderContext.Append($"{visibility}{declaration.Name} : ");
            LuaTypeRenderer.RenderType(tableFieldInfo.DeclarationType, renderContext);
            if (tableFieldInfo.TableFieldPtr.ToNode(renderContext.SearchContext) is
                { IsValue: false, Value: LuaLiteralExprSyntax expr })
            {
                RenderLiteral(expr, renderContext);
            }
        });

        LuaCommentRenderer.RenderTableFieldComment(tableFieldInfo, renderContext);
    }

    private static void RenderNamedTypeDeclaration(LuaDeclaration declaration, NamedTypeInfo namedTypeInfo,
        LuaRenderContext renderContext)
    {
        renderContext.WrapperLua(() =>
        {
            if (namedTypeInfo.DeclarationType is LuaNamedType namedType)
            {
                var namedTypeKind = namedType.GetTypeKind(renderContext.SearchContext);
                switch (namedTypeKind)
                {
                    case NamedTypeKind.Alias:
                        renderContext.Append("(alias) ");
                        break;
                    case NamedTypeKind.Enum:
                        renderContext.Append("(enum) ");
                        break;
                    case NamedTypeKind.Interface:
                        renderContext.Append("(interface) ");
                        break;
                    case NamedTypeKind.Class:
                        renderContext.Append("(class) ");
                        break;
                    default:
                        renderContext.Append("(type) ");
                        break;
                }
            }

            LuaTypeRenderer.RenderType(namedTypeInfo.DeclarationType, renderContext);
            LuaTypeRenderer.RenderDefinedType(namedTypeInfo.DeclarationType, renderContext);
        });

        LuaCommentRenderer.RenderTypeComment(namedTypeInfo, renderContext);
    }

    private static void RenderTypeIndexDeclaration(LuaDeclaration declaration, TypeIndexInfo typeIndexInfo,
        LuaRenderContext renderContext)
    {
        renderContext.WrapperLua(() =>
        {
            renderContext.Append($"(index) ");
            LuaTypeRenderer.RenderType(typeIndexInfo.KeyType, renderContext);
            renderContext.Append(" : ");
            LuaTypeRenderer.RenderType(typeIndexInfo.ValueType, renderContext);
        });
    }

    private static void RenderIndexDeclaration(LuaDeclaration declaration, IndexInfo indexInfo,
        LuaRenderContext renderContext)
    {
        renderContext.WrapperLua(() =>
        {
            var visibility = declaration.Visibility switch
            {
                DeclarationVisibility.Protected => "protected ",
                DeclarationVisibility.Private => "private ",
                _ => ""
            };
            renderContext.Append($"{visibility}(field) {declaration.Name} : ");
            LuaTypeRenderer.RenderType(indexInfo.DeclarationType, renderContext);
            var valueExpr = indexInfo.ValueExprPtr.ToNode(renderContext.SearchContext);
            if (valueExpr is LuaLiteralExprSyntax literalExpr)
            {
                RenderLiteral(literalExpr, renderContext);
            }
        });

        LuaCommentRenderer.RenderDeclarationStatComment(declaration, renderContext);
    }
}
