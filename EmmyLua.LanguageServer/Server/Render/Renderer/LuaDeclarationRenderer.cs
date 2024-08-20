using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;
using EmmyLua.CodeAnalysis.Type.Types;

namespace EmmyLua.LanguageServer.Server.Render.Renderer;

public static class LuaDeclarationRenderer
{
    public static void RenderDeclaration(LuaSymbol symbol, LuaRenderContext renderContext)
    {
        switch (symbol.Info)
        {
            case LocalInfo localInfo:
            {
                RenderLocalDeclaration(symbol, localInfo, renderContext);
                break;
            }
            case GlobalInfo globalInfo:
            {
                RenderGlobalDeclaration(symbol, globalInfo, renderContext);
                break;
            }
            case MethodInfo methodInfo:
            {
                RenderMethodDeclaration(symbol, methodInfo, renderContext);
                break;
            }
            case ParamInfo paramInfo:
            {
                RenderParamDeclaration(symbol, paramInfo, renderContext);
                break;
            }
            case DocFieldInfo docFieldInfo:
            {
                RenderDocFieldDeclaration(symbol, docFieldInfo, renderContext);
                break;
            }
            case TableFieldInfo tableFieldInfo:
            {
                RenderTableFieldDeclaration(symbol, tableFieldInfo, renderContext);
                break;
            }
            case NamedTypeInfo namedTypeInfo:
            {
                RenderNamedTypeDeclaration(symbol, namedTypeInfo, renderContext);
                break;
            }
            case TypeIndexInfo typeIndexInfo:
            {
                RenderTypeIndexDeclaration(symbol, typeIndexInfo, renderContext);
                break;
            }
            case IndexInfo indexInfo:
            {
                RenderIndexDeclaration(symbol, indexInfo, renderContext);
                break;
            }
        }
    }

    private static void RenderInClass(LuaIndexExprSyntax indexExpr, LuaRenderContext renderContext)
    {
        var prefixType = renderContext.SearchContext.Infer(indexExpr.PrefixExpr);
        if (!prefixType.IsSameType(Builtin.Unknown, renderContext.SearchContext))
        {
            RenderBelongType(prefixType, renderContext);
        }
    }

    private static void RenderBelongType(LuaType prefixType, LuaRenderContext renderContext)
    {
        if (!prefixType.IsSameType(Builtin.Unknown, renderContext.SearchContext))
        {
            var parentTypeDescription = "class";
            if (prefixType is LuaNamedType namedType)
            {
                var typeInfo = renderContext.SearchContext.Compilation.TypeManager.FindTypeInfo(namedType);
                var namedTypeKind = typeInfo?.Kind;
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

    private static void RenderLocalDeclaration(LuaSymbol symbol, LocalInfo localInfo,
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
            renderContext.Append($"local {symbol.Name}{attrib} : ");
            LuaTypeRenderer.RenderType(symbol.Type, renderContext);
            LuaTypeRenderer.RenderDefinedType(symbol.Type, renderContext);
        });

        LuaCommentRenderer.RenderDeclarationStatComment(symbol, renderContext);
    }

    private static void RenderGlobalDeclaration(LuaSymbol symbol, GlobalInfo globalInfo,
        LuaRenderContext renderContext)
    {
        renderContext.WrapperLua(() =>
        {
            renderContext.Append($"global {symbol.Name}: ");
            LuaTypeRenderer.RenderType(symbol.Type, renderContext);
            LuaTypeRenderer.RenderDefinedType(symbol.Type, renderContext);
        });

        LuaCommentRenderer.RenderDeclarationStatComment(symbol, renderContext);
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

    private static void RenderMethodDeclaration(LuaSymbol symbol, MethodInfo methodInfo,
        LuaRenderContext renderContext)
    {
        renderContext.EnableAliasRender();
        if (symbol.IsLocal)
        {
            renderContext.WrapperLua(() =>
            {
                renderContext.Append($"local function {symbol.Name}");
                LuaTypeRenderer.RenderFunc(symbol.Type, renderContext);
            });
        }
        else
        {
            renderContext.WrapperLua(() =>
            {
                renderContext.Append($"function {symbol.Name}");
                LuaTypeRenderer.RenderFunc(symbol.Type, renderContext);
            });

            if (methodInfo.IndexPtr.ToNode(renderContext.SearchContext) is { } indexExpr)
            {
                RenderInClass(indexExpr, renderContext);
            }
        }

        LuaCommentRenderer.RenderDeclarationStatComment(symbol, renderContext);
        LuaCommentRenderer.RenderFunctionDocComment(methodInfo, renderContext);
    }

    private static void RenderParamDeclaration(LuaSymbol symbol, ParamInfo paramInfo,
        LuaRenderContext renderContext)
    {
        renderContext.EnableAliasRender();
        if (symbol.Type is { } declarationType)
        {
            renderContext.WrapperLua(() =>
            {
                renderContext.Append($"(parameter) {symbol.Name} : ");
                LuaTypeRenderer.RenderType(declarationType, renderContext);
            });
        }
        else
        {
            renderContext.WrapperLua(() => { renderContext.AppendLine($"(parameter) {symbol.Name}"); });
        }

        LuaCommentRenderer.RenderParamComment(symbol, renderContext);
    }

    private static void RenderDocFieldDeclaration(LuaSymbol symbol, DocFieldInfo docFieldInfo,
        LuaRenderContext renderContext)
    {
        renderContext.WrapperLua(() =>
        {
            var visibility = symbol.Visibility switch
            {
                SymbolVisibility.Public => "",
                SymbolVisibility.Protected => "protected ",
                SymbolVisibility.Private => "private ",
                _ => ""
            };
            renderContext.Append($"{visibility}(field) {symbol.Name} : ");
            LuaTypeRenderer.RenderType(symbol.Type, renderContext);
        });
        LuaCommentRenderer.RenderDocFieldComment(docFieldInfo, renderContext);
    }

    private static void RenderTableFieldDeclaration(LuaSymbol symbol, TableFieldInfo tableFieldInfo,
        LuaRenderContext renderContext)
    {
        renderContext.WrapperLua(() =>
        {
            var visibility = symbol.Visibility switch
            {
                SymbolVisibility.Public => "",
                SymbolVisibility.Protected => "protected ",
                SymbolVisibility.Private => "private ",
                _ => ""
            };

            renderContext.Append($"{visibility}{symbol.Name} : ");
            LuaTypeRenderer.RenderType(symbol.Type, renderContext);
            if (tableFieldInfo.TableFieldPtr.ToNode(renderContext.SearchContext) is
                { IsValue: false, Value: LuaLiteralExprSyntax expr })
            {
                RenderLiteral(expr, renderContext);
            }
        });

        LuaCommentRenderer.RenderTableFieldComment(tableFieldInfo, renderContext);
    }

    private static void RenderNamedTypeDeclaration(LuaSymbol symbol, NamedTypeInfo namedTypeInfo,
        LuaRenderContext renderContext)
    {
        renderContext.WrapperLua(() =>
        {
            if (symbol.Type is LuaNamedType namedType)
            {
                var typeInfo = renderContext.SearchContext.Compilation.TypeManager.FindTypeInfo(namedType);
                var namedTypeKind = typeInfo?.Kind;
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

            LuaTypeRenderer.RenderType(symbol.Type, renderContext);
            LuaTypeRenderer.RenderDefinedType(symbol.Type, renderContext);
        });

        LuaCommentRenderer.RenderTypeComment(namedTypeInfo, renderContext);
    }

    private static void RenderTypeIndexDeclaration(LuaSymbol symbol, TypeIndexInfo typeIndexInfo,
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

    private static void RenderIndexDeclaration(LuaSymbol symbol, IndexInfo indexInfo,
        LuaRenderContext renderContext)
    {
        renderContext.WrapperLua(() =>
        {
            var visibility = symbol.Visibility switch
            {
                SymbolVisibility.Protected => "protected ",
                SymbolVisibility.Private => "private ",
                _ => ""
            };
            renderContext.Append($"{visibility}(field) {symbol.Name} : ");
            LuaTypeRenderer.RenderType(symbol.Type, renderContext);
            var valueExpr = indexInfo.ValueExprPtr.ToNode(renderContext.SearchContext);
            if (valueExpr is LuaLiteralExprSyntax literalExpr)
            {
                RenderLiteral(literalExpr, renderContext);
            }
        });

        LuaCommentRenderer.RenderDeclarationStatComment(symbol, renderContext);
    }
}
