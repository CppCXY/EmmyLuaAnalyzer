using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.DetailType;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Render.Renderer;

internal static class LuaTypeRenderer
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
            InnerRenderDetailType(namedType, renderContext);
            return;
        }

        renderContext.Append("unknown");
    }

    private static void InnerRenderDetailType(LuaNamedType namedType, LuaRenderContext renderContext)
    {
        var detailType = namedType.GetDetailType(renderContext.SearchContext);
        if (detailType is AliasDetailType {OriginType: { } originType})
        {
            renderContext.Append($"alias {namedType.Name}");
            renderContext.AddSeparator();
            if (originType is LuaAggregateType aggregateType)
            {
                renderContext.AppendLine();
                foreach (var typeDeclaration in aggregateType.Declarations)
                {
                    renderContext.Append("| ");
                    InnerRenderType(typeDeclaration.Info.DeclarationType!, renderContext, 1);
                    if (typeDeclaration.Info is AggregateMemberInfo {TypePtr: { } typePtr} &&
                        typePtr.ToNode(renderContext.SearchContext) is {Description: { } description})
                    {
                        renderContext.Append($" {description.RepresentText}");
                    }

                    renderContext.AppendLine();
                }
            }
            else
            {
                renderContext.Append(" = ");
                InnerRenderType(originType, renderContext, 1);
            }
        }
        else if (detailType is ClassDetailType classType)
        {
            renderContext.Append($"class {classType.Name}");
            var generics = classType.Generics;
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
                    if (generic.Info.DeclarationType is { } baseType)
                    {
                        renderContext.Append(':');
                        InnerRenderType(baseType, renderContext, 1);
                    }
                }

                renderContext.Append('>');
            }

            var supers = classType.Supers;
            if (supers.Count > 0)
            {
                renderContext.Append(" extends ");
                for (var i = 0; i < supers.Count; i++)
                {
                    if (i > 0)
                    {
                        renderContext.Append(',');
                    }

                    InnerRenderType(supers[i], renderContext, 1);
                }
            }
        }
        else if (detailType is InterfaceDetailType interfaceType)
        {
            renderContext.Append($"interface {interfaceType.Name}");
            var generics = interfaceType.Generics;
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
                    if (generic.Info.DeclarationType is { } baseType)
                    {
                        renderContext.Append(':');
                        InnerRenderType(baseType, renderContext, 1);
                    }
                }

                renderContext.Append('>');
            }

            var supers = interfaceType.Supers;
            if (supers.Count > 0)
            {
                renderContext.Append(" extends ");
                for (var i = 0; i < supers.Count; i++)
                {
                    if (i > 0)
                    {
                        renderContext.Append(',');
                    }

                    InnerRenderType(supers[i], renderContext, 1);
                }
            }
        }
        else if (detailType is EnumDetailType enumType)
        {
            renderContext.Append($"enum {enumType.Name}");
        }
        else
        {
            InnerRenderType(namedType, renderContext, 1);
        }
    }

    private static void InnerRenderType(LuaType type, LuaRenderContext renderContext, int level)
    {
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
            default:
            {
                renderContext.Append("unknown");
                break;
            }
        }
    }

    private static void RenderNamedType(LuaNamedType namedType, LuaRenderContext renderContext, int level)
    {

        var detailType = namedType.GetDetailType(renderContext.SearchContext);
        if (level == 0 && renderContext.Feature.ExpandAlias)
        {
            if (detailType is AliasDetailType {OriginType: { } originType})
            {
                InnerRenderType(originType, renderContext, 1);
                return;
            }
        }

        var name = namedType.Name;
        if (name.Length != 0 && char.IsDigit(name[0]))
        {
            renderContext.Append("ambiguous");
        }
        else
        {
            renderContext.Append(namedType.Name);
        }
    }

    private static void RenderArrayType(LuaArrayType arrayType, LuaRenderContext renderContext, int level)
    {
        InnerRenderType(arrayType.BaseType, renderContext, level + 1);
        renderContext.Append("[]");
    }

    private static void RenderUnionType(LuaUnionType unionType, LuaRenderContext renderContext, int level)
    {
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

            InnerRenderType(tupleType.TupleDeclaration[i].Info.DeclarationType!, renderContext, level + 1);
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
            var genericParameters = genericMethodType.GenericParameterDeclarations;
            renderContext.Append('<');
            for (var i = 0; i < genericParameters.Count; i++)
            {
                if (i > 0)
                {
                    renderContext.Append(',');
                }

                renderContext.Append(genericParameters[i].Name);
                if (genericParameters[i].Info is GenericParamInfo {Variadic: true})
                {
                    renderContext.Append("...");
                }
            }

            renderContext.Append('>');
        }

        var mainSignature = methodType.MainSignature;

        renderContext.Append('(');
        for (var i = 0; i < mainSignature.Parameters.Count; i++)
        {
            if (i > 0)
            {
                renderContext.Append(", ");
            }

            var parameter = mainSignature.Parameters[i];
            renderContext.Append(parameter.Name);
            renderContext.Append(':');
            InnerRenderType(parameter.Info.DeclarationType ?? Builtin.Any, renderContext, 0);
        }

        renderContext.Append(')');

        renderContext.Append(" -> ");
        InnerRenderType(mainSignature.ReturnType, renderContext, 0);
    }

    private static void RenderMultiReturnType(LuaMultiReturnType multiReturnType, LuaRenderContext renderContext, int level)
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

        if (docTableType.DocTablePtr.ToNode(renderContext.SearchContext) is {Body: { } body})
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
                    case {NameField: { } nameField, Type: { } type1}:
                    {
                        var type = renderContext.SearchContext.Infer(type1);
                        renderContext.Append($"{nameField.RepresentText}:");
                        RenderType(type, renderContext);
                        break;
                    }
                    case {IntegerField: { } integerField, Type: { } type2}:
                    {
                        var type = renderContext.SearchContext.Infer(type2);
                        renderContext.Append($"[{integerField.Value}]:");
                        RenderType(type, renderContext);
                        break;
                    }
                    case {StringField: { } stringField, Type: { } type3}:
                    {
                        var type = renderContext.SearchContext.Infer(type3);
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
            InnerRenderType(typeDeclaration.Info.DeclarationType!, renderContext, 1);
        }
    }
}
