﻿using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

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
            if (type.IsSameType(Builtin.Nil, renderContext.SearchContext) ||
                type.IsSameType(Builtin.Unknown, renderContext.SearchContext) ||
                type.IsSameType(Builtin.Any, renderContext.SearchContext) ||
                type.IsSameType(Builtin.UserData, renderContext.SearchContext))
            {
                return;
            }

            InnerRenderDetailType(namedType, renderContext);
        }
    }

    // public static void RenderAliasMember(string aliasName, LuaAggregateType aggregateType,
    //     LuaRenderContext renderContext)
    // {
    //     renderContext.AppendLine();
    //     // renderContext.AddSeparator();
    //     renderContext.WrapperLua(() =>
    //     {
    //         renderContext.Append($"{aliasName}:\n");
    //         foreach (var typeDeclaration in aggregateType.Declarations)
    //         {
    //             if (typeDeclaration.Type is not null)
    //             {
    //                 renderContext.Append("    | ");
    //                 InnerRenderType(typeDeclaration.Type, renderContext, 1);
    //                 if (typeDeclaration is { Info: AggregateMemberInfo { TypePtr: { } typePtr } } &&
    //                     typePtr.ToNode(renderContext.SearchContext) is { Description: { } description })
    //                 {
    //                     renderContext.Append(" --");
    //                     foreach (var token in description.ChildrenWithTokens)
    //                     {
    //                         if (token is LuaSyntaxToken { Kind: LuaTokenKind.TkDocDetail, RepresentText: { } text })
    //                         {
    //                             if (text.StartsWith('@') || text.StartsWith('#'))
    //                             {
    //                                 renderContext.Append(text[1..]);
    //                             }
    //                             else
    //                             {
    //                                 renderContext.Append(text);
    //                             }
    //                         }
    //                     }
    //                 }
    //                 
    //                 renderContext.AppendLine();
    //             }
    //         }
    //     });
    // }

    private static void InnerRenderDetailType(LuaNamedType namedType, LuaRenderContext renderContext)
    {
        var typeInfo = renderContext.SearchContext.Compilation.TypeManager.FindTypeInfo(namedType);
        if (typeInfo is null)
        {
            return;
        }

        var namedTypeKind = typeInfo.Kind;
        if (namedTypeKind == NamedTypeKind.Alias)
        {
            var originType = typeInfo.BaseType;
            // if (originType is LuaAggregateType)
            // {
            //     renderContext.AddAliasExpand(namedType);
            // }
            // else
            if (originType is not null)
            {
                renderContext.Append(" = ");
                InnerRenderType(originType, renderContext, 1);
            }
        }
        // else if (namedTypeKind is NamedTypeKind.Class or NamedTypeKind.Interface)
        // {
            // var generics = typeInfo.GenericParameters ?? [];
            // var supers = typeInfo.Supers ?? [];
            // RenderClassOrInterface(namedType.Name, generics, supers, renderContext);
        // }
        else if (namedTypeKind == NamedTypeKind.Enum)
        {
            var baseType = typeInfo.BaseType;
            if (baseType is not null)
            {
                renderContext.Append(" extends ");
                InnerRenderType(baseType, renderContext, 1);
            }
        }
    }

    private static void RenderClassOrInterface(string name, List<LuaSymbol> generics, List<LuaNamedType> supers,
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
    }

    private static void InnerRenderType(LuaType? type, LuaRenderContext renderContext, int level)
    {
        // 防止递归过深
        if (level > 10)
        {
            return;
        }

        if (type is not null)
        {
            renderContext.AddTypeLink(type);
        }

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
            // case LuaAggregateType aggregateType:
            // {
            //     RenderAggregateType(aggregateType, renderContext, level);
            //     break;
            // }
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
            case LuaVariadicType variadicType:
            {
                renderContext.Append("...");
                RenderType(variadicType.BaseType, renderContext);
                break;
            }
            case LuaExpandTemplate expandType:
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
            case LuaStringTemplate templateType:
            {
                renderContext.Append($"{templateType.PrefixName}<{templateType.TemplateName}>");
                break;
            }
            case LuaElementRef luaElementType:
            {
                RenderVariableRefType(luaElementType, renderContext, level);
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

    private static void RenderVariableRefType(LuaElementRef variableRefRef, LuaRenderContext renderContext,
        int level)
    {
        var typeInfo = renderContext.SearchContext.Compilation.TypeManager.FindTypeInfo(variableRefRef.Id);
        var baseType = typeInfo?.BaseType;
        if (baseType is null)
        {
            var element = variableRefRef.ToSyntaxElement(renderContext.SearchContext);
            if (element is LuaTableExprSyntax)
            {
                renderContext.Append("table");
            }
            else if (element is LuaFuncStatSyntax)
            {
                renderContext.Append("fun");
            }
            else
            {
                renderContext.Append("unknown");
            }
            
            return;
        }

        InnerRenderType(baseType, renderContext, level + 1);
    }

    private static void RenderGlobalNameType(GlobalNameType globalNameType, LuaRenderContext renderContext, int level)
    {
        var globalSymbol = renderContext.SearchContext.Compilation.TypeManager.GetGlobalSymbol(globalNameType.Name);
        if (globalSymbol?.Type is LuaNamedType namedType)
        {
            InnerRenderType(namedType, renderContext, level + 1);
        }
        else
        {
            var globalInfo = renderContext.SearchContext.Compilation.TypeManager.FindTypeInfo(globalNameType.Name);
            if (globalInfo?.BaseType is not null)
            {
                InnerRenderType(globalInfo.BaseType, renderContext, level + 1);
            }
            else
            {
                renderContext.Append($"global {globalNameType.Name}");
            }
        }
    }

    private static void RenderNamedType(LuaNamedType namedType, LuaRenderContext renderContext, int level)
    {
        var typeInfo = renderContext.SearchContext.Compilation.TypeManager.FindTypeInfo(namedType);
        if (typeInfo is null)
        {
            renderContext.Append(namedType.Name);
            return;
        }

        var namedTypeKind = typeInfo.Kind;
        if (level == 0 && renderContext.Feature.ExpandAlias)
        {
            if (namedTypeKind == NamedTypeKind.Alias)
            {
                var originType = typeInfo.BaseType;
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

        if (unionType.TypeList.Count == 2 && unionType.TypeList.Contains(Builtin.Nil))
        {
            var newType = unionType.Remove(Builtin.Nil, renderContext.SearchContext);
            InnerRenderType(newType, renderContext, level + 1);
            renderContext.Append('?');
            return;
        }

        var count = 0;
        foreach (var luaType in unionType.TypeList)
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
        for (var i = 0; i < tupleType.TypeList.Count; i++)
        {
            if (i > 0)
            {
                renderContext.Append(',');
            }

            InnerRenderType(tupleType.TypeList[i], renderContext, level + 1);
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
        if (signature.ReturnType.IsSameType(Builtin.Nil, renderContext.SearchContext))
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
        if (signature.ReturnType.IsSameType(Builtin.Nil, renderContext.SearchContext))
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
        if (genericType.Name == "namespace")
        {
            renderContext.Append("namespace");
            if (genericType.GenericArgs.FirstOrDefault() is LuaStringLiteralType namespaceString)
            {
                renderContext.Append($" {namespaceString.Content}");
            }
            return;
        }
        
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

    // private static void RenderLuaDocTableType(LuaDocTableType docTableType, LuaRenderContext renderContext,
    //     int level)
    // {
    //     renderContext.Append('{');
    //     if (level > 1)
    //     {
    //         renderContext.Append("...}");
    //         return;
    //     }
    //
    //     if (docTableType.DocTablePtr.ToNode(renderContext.SearchContext) is { Body: { } body })
    //     {
    //         var fieldList = body.FieldList.ToList();
    //         for (var i = 0; i < fieldList.Count; i++)
    //         {
    //             if (i > 0)
    //             {
    //                 renderContext.Append(", ");
    //             }
    //
    //             if (renderContext.Feature.InHint && i > 2)
    //             {
    //                 renderContext.Append("...");
    //                 break;
    //             }
    //
    //             var field = fieldList[i];
    //             switch (field)
    //             {
    //                 case { NameField: { } nameField, Type: { } type1 }:
    //                 {
    //                     var type = renderContext.SearchContext.InferAndUnwrap(type1);
    //                     renderContext.Append($"{nameField.RepresentText}:");
    //                     RenderType(type, renderContext);
    //                     break;
    //                 }
    //                 case { IntegerField: { } integerField, Type: { } type2 }:
    //                 {
    //                     var type = renderContext.SearchContext.InferAndUnwrap(type2);
    //                     renderContext.Append($"[{integerField.Value}]:");
    //                     RenderType(type, renderContext);
    //                     break;
    //                 }
    //                 case { StringField: { } stringField, Type: { } type3 }:
    //                 {
    //                     var type = renderContext.SearchContext.InferAndUnwrap(type3);
    //                     renderContext.Append($"[{stringField.Value}]:");
    //                     RenderType(type, renderContext);
    //                     break;
    //                 }
    //                 // case { TypeField: { } typeField, Type: { } type4 }:
    //                 // {
    //                 //     // var keyType = context.Infer(typeField);
    //                 //     // var valueType = context.Infer(type4);
    //                 //
    //                 //     break;
    //                 // }
    //             }
    //         }
    //     }
    //
    //     renderContext.Append('}');
    // }

    // private static void RenderAggregateType(LuaAggregateType aggregateType, LuaRenderContext renderContext, int level)
    // {
    //     for (var index = 0; index < aggregateType.Declarations.Count; index++)
    //     {
    //         var typeDeclaration = aggregateType.Declarations[index];
    //         if (index > 0)
    //         {
    //             renderContext.Append('|');
    //         }
    //
    //         InnerRenderType(typeDeclaration.Type, renderContext, 1);
    //     }
    // }
}