using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.LanguageServer.Server.Render.Renderer;

public static class LuaTypeRenderer
{
    public static void RenderType(LuaType? type, LuaRenderContext renderContext)
    {
        if (type is null)
        {
            renderContext.Append("unknown");
            return;
        }

        InnerRenderType(type, renderContext, 0);
    }

    public static void RenderFunc(LuaType? type, LuaRenderContext renderContext)
    {
        if (type is LuaMethodType methodType)
        {
            RenderFunctionType(methodType, renderContext, 0, true);
            return;
        }

        renderContext.Append("() -> void");
    }

    public static void RenderDefinedType(LuaType? type, LuaRenderContext renderContext)
    {
        if (type is LuaNamedType namedType)
        {
            if (type.Equals(Builtin.Nil) || type.Equals(Builtin.Unknown) || type.Equals(Builtin.Any) ||
                type.Equals(Builtin.UserData))
            {
                return;
            }

            InnerRenderDetailType(namedType, renderContext);
        }
    }

    public static void RenderAliasMember(string aliasName, LuaAggregateType aggregateType,
        LuaRenderContext renderContext)
    {
        renderContext.AppendLine();
        // renderContext.AddSeparator();
        renderContext.WrapperLua(() =>
        {
            renderContext.Append($"{aliasName}:\n");
            foreach (var typeDeclaration in aggregateType.Declarations)
            {
                renderContext.Append("    | ");
                InnerRenderType(typeDeclaration.Type, renderContext, 1);
                if (typeDeclaration is LuaDeclaration { Info: AggregateMemberInfo { TypePtr: { } typePtr } } &&
                    typePtr.ToNode(renderContext.SearchContext) is { Description: { } description })
                {
                    renderContext.Append(" --");
                    foreach (var token in description.ChildrenWithTokens)
                    {
                        if (token is LuaSyntaxToken { Kind: LuaTokenKind.TkDocDetail, RepresentText: { } text })
                        {
                            if (text.StartsWith('@') || text.StartsWith('#'))
                            {
                                renderContext.Append(text[1..]);
                            }
                            else
                            {
                                renderContext.Append(text);
                            }
                        }
                    }
                }

                renderContext.AppendLine();
            }
        });
    }

    private static void InnerRenderDetailType(LuaNamedType namedType, LuaRenderContext renderContext)
    {
        var namedTypeKind = namedType.GetTypeKind(renderContext.SearchContext);
        if (namedTypeKind == NamedTypeKind.Alias)
        {
            var originType = renderContext.SearchContext.Compilation.Db.QueryAliasOriginTypes(namedType.Name);
            if (originType is LuaAggregateType)
            {
                renderContext.AddAliasExpand(namedType);
            }
            else if (originType is not null)
            {
                renderContext.Append(" = ");
                InnerRenderType(originType, renderContext, 1);
            }
        }
        else if (namedTypeKind is NamedTypeKind.Class or NamedTypeKind.Interface)
        {
            var generics = renderContext.SearchContext.Compilation.Db.QueryGenericParams(namedType.Name).ToList();
            var supers = renderContext.SearchContext.Compilation.Db.QuerySupers(namedType.Name).ToList();
            RenderClassOrInterface(namedType.Name, generics, supers, renderContext);
        }
        else if (namedTypeKind == NamedTypeKind.Enum)
        {
            var baseType = renderContext.SearchContext.Compilation.Db.QuerySupers(namedType.Name).FirstOrDefault();
            if (baseType is not null)
            {
                renderContext.Append(" extends ");
                InnerRenderType(baseType, renderContext, 1);
            }
        }
    }

    private static void RenderClassOrInterface(string name, List<IDeclaration> generics, List<LuaType> supers,
        LuaRenderContext renderContext)
    {
        if (generics.Count > 0)
        {
            renderContext.Append('<');
            for (var i = 0; i < generics.Count; i++)
            {
                if (i > 0)
                {
                    renderContext.Append(',');
                }

                var generic = generics[i];
                renderContext.Append(generic.Name);
                if (generic.Type is { } baseType)
                {
                    renderContext.Append(':');
                    InnerRenderType(baseType, renderContext, 1);
                }
            }

            renderContext.Append('>');
        }

        // if (supers.Count > 0)
        // {
        //     renderContext.Append(" extends ");
        //     for (var i = 0; i < supers.Count; i++)
        //     {
        //         if (i > 0)
        //         {
        //             renderContext.Append(',');
        //         }
        //
        //         InnerRenderType(supers[i], renderContext, 1);
        //     }
        // }

        // 似乎没有展开的意义
        // var members = renderContext.SearchContext.GetMembers(new LuaNamedType(name)).ToList();
        // if (members.Count == 0)
        // {
        //     return;
        // }
        //
        // // 只渲染20个
        // var count = 0;
        // renderContext.Append(" {\n");
        // foreach (var member in members)
        // {
        //     if (count > 20)
        //     {
        //         renderContext.Append(",    \n...");
        //         break;
        //     }
        //
        //     if (count > 0)
        //     {
        //         renderContext.Append(",\n");
        //     }
        //
        //     renderContext.Append("    ");
        //     renderContext.Append(member.Name);
        //     renderContext.Append(": ");
        //     InnerRenderType(member.Type, renderContext, 1);
        //
        //     count++;
        // }
        //
        // renderContext.Append("\n}");
    }

    private static void InnerRenderType(LuaType type, LuaRenderContext renderContext, int level)
    {
        // 防止递归过深
        if (level > 10)
        {
            return;
        }

        renderContext.AddTypeLink(type);
        switch (type)
        {
            case LuaArrayType arrayType:
            {
                RenderArrayType(arrayType, renderContext, level);
                break;
            }
            case LuaUnionType unionType:
            {
                RenderUnionType(unionType, renderContext, level);
                break;
            }
            case LuaAggregateType aggregateType:
            {
                RenderAggregateType(aggregateType, renderContext, level);
                break;
            }
            case LuaTupleType tupleType:
            {
                RenderTupleType(tupleType, renderContext, level);
                break;
            }
            case LuaMethodType methodType:
            {
                RenderFunctionType(methodType, renderContext, level, false);
                break;
            }
            case LuaMultiReturnType multiReturnType:
            {
                RenderMultiReturnType(multiReturnType, renderContext, level);
                break;
            }
            case LuaStringLiteralType stringLiteralType:
            {
                if (stringLiteralType.Content.StartsWith('\'') || stringLiteralType.Content.StartsWith('"'))
                {
                    renderContext.Append(stringLiteralType.Content);
                    break;
                }

                renderContext.Append('"');
                renderContext.Append(stringLiteralType.Content);
                renderContext.Append('"');
                break;
            }
            case LuaIntegerLiteralType integerLiteralType:
            {
                renderContext.Append(integerLiteralType.Value.ToString());
                break;
            }
            case LuaGenericType genericType:
            {
                RenderGeneric(genericType, renderContext, level);
                break;
            }
            case LuaTableLiteralType:
            {
                renderContext.Append("table");
                break;
            }
            case LuaDocTableType docTableType:
            {
                RenderLuaDocTableType(docTableType, renderContext, level);
                break;
            }
            case LuaVariadicType variadicType:
            {
                renderContext.Append("...");
                RenderType(variadicType.BaseType, renderContext);
                break;
            }
            case LuaExpandType expandType:
            {
                renderContext.Append(expandType.Name);
                renderContext.Append("...");
                break;
            }
            case LuaNamedType namedType:
            {
                RenderNamedType(namedType, renderContext, level);
                break;
            }
            case LuaTemplateType templateType:
            {
                renderContext.Append($"{templateType.PrefixName}<{templateType.TemplateName}>");
                break;
            }
            case LuaVariableRefType variableRefType:
            {
                RenderVariableRefType(variableRefType, renderContext, level);
                break;
            }
            case GlobalNameType globalNameType:
            {
                RenderGlobalNameType(globalNameType, renderContext, level);
                break;
            }
            default:
            {
                renderContext.Append("unknown");
                break;
            }
        }
    }

    private static void RenderVariableRefType(LuaVariableRefType variableRefType, LuaRenderContext renderContext,
        int level)
    {
        var relatedType = renderContext.SearchContext.Compilation.Db.QueryTypeFromId(variableRefType.Id);
        if (relatedType is null)
        {
            renderContext.Append("ambiguous");
            return;
        }

        InnerRenderType(relatedType, renderContext, level + 1);
    }

    private static void RenderGlobalNameType(GlobalNameType globalNameType, LuaRenderContext renderContext, int level)
    {
        var relatedType = renderContext.SearchContext.Compilation.Db.QueryRelatedGlobalType(globalNameType.Name);
        if (relatedType is null)
        {
            renderContext.Append($"global {globalNameType.Name}");
            return;
        }

        InnerRenderType(relatedType, renderContext, level + 1);
    }

    private static void RenderNamedType(LuaNamedType namedType, LuaRenderContext renderContext, int level)
    {
        var namedTypeKind = namedType.GetTypeKind(renderContext.SearchContext);
        if (level == 0 && renderContext.Feature.ExpandAlias)
        {
            if (namedTypeKind == NamedTypeKind.Alias)
            {
                var originType = renderContext.SearchContext.Compilation.Db.QueryAliasOriginTypes(namedType.Name);
                if (originType is not null)
                {
                    InnerRenderType(originType, renderContext, 1);
                    return;
                }
            }
        }

        renderContext.Append(namedType.Name);
    }

    private static void RenderArrayType(LuaArrayType arrayType, LuaRenderContext renderContext, int level)
    {
        InnerRenderType(arrayType.BaseType, renderContext, level + 1);
        renderContext.Append("[]");
    }

    private static void RenderUnionType(LuaUnionType unionType, LuaRenderContext renderContext, int level)
    {
        if (level > 3)
        {
            renderContext.Append("[...]");
            return;
        }

        if (unionType.UnionTypes.Count == 2 && unionType.UnionTypes.Contains(Builtin.Nil))
        {
            var newType = unionType.Remove(Builtin.Nil);
            InnerRenderType(newType, renderContext, level + 1);
            renderContext.Append('?');
            return;
        }

        var count = 0;
        foreach (var luaType in unionType.UnionTypes)
        {
            if (count > 0)
            {
                renderContext.Append('|');
            }

            if (count > 2 && renderContext.Feature.InHint)
            {
                renderContext.Append("...");
                break;
            }

            InnerRenderType(luaType, renderContext, level + 1);
            count++;
        }
    }

    private static void RenderTupleType(LuaTupleType tupleType, LuaRenderContext renderContext, int level)
    {
        renderContext.Append('[');
        for (var i = 0; i < tupleType.TupleDeclaration.Count; i++)
        {
            if (i > 0)
            {
                renderContext.Append(',');
            }

            InnerRenderType(tupleType.TupleDeclaration[i].Type, renderContext, level + 1);
        }

        renderContext.Append(']');
    }

    private static void RenderFunctionType(LuaMethodType methodType, LuaRenderContext renderContext, int level,
        bool skipFun)
    {
        if (!skipFun)
        {
            renderContext.Append("fun");
        }

        // show generic
        if (methodType is LuaGenericMethodType genericMethodType)
        {
            var genericParameters = genericMethodType.GenericParamDecls;
            renderContext.Append('<');
            for (var i = 0; i < genericParameters.Count; i++)
            {
                if (i > 0)
                {
                    renderContext.Append(',');
                }

                renderContext.Append(genericParameters[i].Name);
            }

            renderContext.Append('>');
        }

        var mainSignature = methodType.MainSignature;
        if (renderContext.Feature.InHover && !renderContext.InSignature)
        {
            renderContext.WithSignature(() => { RenderSignatureForHover(mainSignature, renderContext, level); });
        }
        else
        {
            RenderSignature(mainSignature, renderContext, level);
        }
    }

    private static void RenderSignature(LuaSignature signature, LuaRenderContext renderContext, int level)
    {
        renderContext.Append('(');
        for (var i = 0; i < signature.Parameters.Count; i++)
        {
            if (i > 0)
            {
                renderContext.Append(", ");
            }

            var parameter = signature.Parameters[i];
            renderContext.Append(parameter.Name);
            renderContext.Append(':');
            InnerRenderType(parameter.Type, renderContext, level + 1);
        }

        renderContext.Append(')');

        renderContext.Append(" -> ");
        if (signature.ReturnType.Equals(Builtin.Nil))
        {
            renderContext.Append("void");
        }
        else
        {
            InnerRenderType(signature.ReturnType, renderContext, level + 1);
        }
    }

    private static void RenderSignatureForHover(LuaSignature signature, LuaRenderContext renderContext, int level)
    {
        renderContext.Append('(');
        var chopDown = signature.Parameters.Count > 0;
        if (!chopDown)
        {
            renderContext.Append(')');
        }
        else
        {
            renderContext.AppendLine();
            for (var i = 0; i < signature.Parameters.Count; i++)
            {
                if (i > 0)
                {
                    renderContext.Append(",\n");
                }

                var parameter = signature.Parameters[i];
                renderContext.Append($"    {parameter.Name}");
                renderContext.Append(':');
                InnerRenderType(parameter.Type, renderContext, level + 1);
            }

            renderContext.Append("\n)");
        }

        renderContext.Append(" -> ");
        if (signature.ReturnType.Equals(Builtin.Nil))
        {
            renderContext.Append("void");
        }
        else
        {
            InnerRenderType(signature.ReturnType, renderContext, level + 1);
        }
    }

    private static void RenderMultiReturnType(LuaMultiReturnType multiReturnType, LuaRenderContext renderContext,
        int level)
    {
        renderContext.Append('(');
        for (var i = 0; i < multiReturnType.GetElementCount(); i++)
        {
            if (i > 0)
            {
                renderContext.Append(',');
            }

            InnerRenderType(multiReturnType.GetElementType(i), renderContext, level + 1);
        }

        renderContext.Append(')');
    }

    private static void RenderGeneric(LuaGenericType genericType, LuaRenderContext renderContext, int level)
    {
        renderContext.Append(genericType.Name);
        renderContext.Append('<');
        for (var i = 0; i < genericType.GenericArgs.Count; i++)
        {
            if (i > 0)
            {
                renderContext.Append(',');
            }

            InnerRenderType(genericType.GenericArgs[i], renderContext, level + 1);
        }

        renderContext.Append('>');
    }

    private static void RenderLuaDocTableType(LuaDocTableType docTableType, LuaRenderContext renderContext,
        int level)
    {
        renderContext.Append('{');
        if (level > 1)
        {
            renderContext.Append("...}");
            return;
        }

        if (docTableType.DocTablePtr.ToNode(renderContext.SearchContext) is { Body: { } body })
        {
            var fieldList = body.FieldList.ToList();
            for (var i = 0; i < fieldList.Count; i++)
            {
                if (i > 0)
                {
                    renderContext.Append(", ");
                }

                if (renderContext.Feature.InHint && i > 2)
                {
                    renderContext.Append("...");
                    break;
                }

                var field = fieldList[i];
                switch (field)
                {
                    case { NameField: { } nameField, Type: { } type1 }:
                    {
                        var type = renderContext.SearchContext.InferAndUnwrap(type1);
                        renderContext.Append($"{nameField.RepresentText}:");
                        RenderType(type, renderContext);
                        break;
                    }
                    case { IntegerField: { } integerField, Type: { } type2 }:
                    {
                        var type = renderContext.SearchContext.InferAndUnwrap(type2);
                        renderContext.Append($"[{integerField.Value}]:");
                        RenderType(type, renderContext);
                        break;
                    }
                    case { StringField: { } stringField, Type: { } type3 }:
                    {
                        var type = renderContext.SearchContext.InferAndUnwrap(type3);
                        renderContext.Append($"[{stringField.Value}]:");
                        RenderType(type, renderContext);
                        break;
                    }
                    // case { TypeField: { } typeField, Type: { } type4 }:
                    // {
                    //     // var keyType = context.Infer(typeField);
                    //     // var valueType = context.Infer(type4);
                    //
                    //     break;
                    // }
                }
            }
        }

        renderContext.Append('}');
    }

    private static void RenderAggregateType(LuaAggregateType aggregateType, LuaRenderContext renderContext, int level)
    {
        for (var index = 0; index < aggregateType.Declarations.Count; index++)
        {
            var typeDeclaration = aggregateType.Declarations[index];
            if (index > 0)
            {
                renderContext.Append('|');
            }

            InnerRenderType(typeDeclaration.Type, renderContext, 1);
        }
    }
}