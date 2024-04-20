using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.DetailType;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Render;

public static class LuaTypeRender
{
    public static string RenderType(LuaType? type, SearchContext context)
    {
        if (type is null)
        {
            return "unknown";
        }

        var sb = new StringBuilder();
        InnerRenderType(type, context, sb, 0);
        return sb.ToString();
    }

    public static string RenderFunc(LuaType? type, SearchContext context)
    {
        if (type is LuaMethodType methodType)
        {
            var sb = new StringBuilder();
            RenderFunctionType(methodType, context, sb, 0, true);
            return sb.ToString();
        }

        return "() -> void";
    }

    public static string RenderDefinedType(LuaType? type, SearchContext context)
    {
        if (type is LuaNamedType namedType)
        {
            var sb = new StringBuilder();
            InnerRenderDetailType(namedType, context, sb, 0);
            return sb.ToString();
        }

        return "unknown";
    }

    private static void InnerRenderDetailType(LuaNamedType namedType, SearchContext context, StringBuilder sb,
        int level)
    {
        var detailType = namedType.GetDetailType(context);
        if (detailType is AliasDetailType { OriginType: { } originType })
        {
            InnerRenderType(originType, context, sb, level + 1);
        }
        else if (detailType is ClassDetailType classType)
        {
            sb.Append(classType.Name);
            var generics = classType.Generics;
            if (generics.Count > 0)
            {
                sb.Append('<');
                for (var i = 0; i < generics.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(',');
                    }

                    var generic = generics[i];
                    sb.Append(generic.Name);
                    if (generic.DeclarationType is { } baseType)
                    {
                        sb.Append(':');
                        InnerRenderType(baseType, context, sb, level + 1);
                    }
                }

                sb.Append('>');
            }

            var supers = classType.Supers;
            if (supers.Count > 0)
            {
                sb.Append(" extends ");
                for (var i = 0; i < supers.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(',');
                    }

                    InnerRenderType(supers[i], context, sb, level + 1);
                }
            }
        }
        else if (detailType is InterfaceDetailType interfaceType)
        {
            sb.Append(interfaceType.Name);
            var generics = interfaceType.Generics;
            if (generics.Count > 0)
            {
                sb.Append('<');
                for (var i = 0; i < generics.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(',');
                    }

                    var generic = generics[i];
                    sb.Append(generic.Name);
                    if (generic.DeclarationType is { } baseType)
                    {
                        sb.Append(':');
                        InnerRenderType(baseType, context, sb, level + 1);
                    }
                }

                sb.Append('>');
            }

            var supers = interfaceType.Supers;
            if (supers.Count > 0)
            {
                sb.Append(" extends ");
                for (var i = 0; i < supers.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(',');
                    }

                    InnerRenderType(supers[i], context, sb, level + 1);
                }
            }
        }
        else if (detailType is EnumDetailType enumType)
        {
            sb.Append($"enum {enumType.Name}");
        }
        else
        {
            InnerRenderType(namedType, context, sb, level);
        }
    }

    private static void InnerRenderType(LuaType type, SearchContext context, StringBuilder sb, int level)
    {
        switch (type)
        {
            case LuaArrayType arrayType:
            {
                RenderArrayType(arrayType, context, sb, level);
                break;
            }
            case LuaUnionType unionType:
            {
                RenderUnionType(unionType, context, sb, level);
                break;
            }
            case LuaTupleType tupleType:
            {
                RenderTupleType(tupleType, context, sb, level);
                break;
            }
            case LuaMethodType methodType:
            {
                RenderFunctionType(methodType, context, sb, level, false);
                break;
            }
            case LuaMultiReturnType multiReturnType:
            {
                RenderMultiReturnType(multiReturnType, context, sb, level);
                break;
            }
            case LuaStringLiteralType stringLiteralType:
            {
                sb.Append('"');
                sb.Append(stringLiteralType.Content);
                sb.Append('"');
                break;
            }
            case LuaIntegerLiteralType integerLiteralType:
            {
                sb.Append(integerLiteralType.Value);
                break;
            }
            case LuaGenericType genericType:
            {
                RenderGeneric(genericType, context, sb, level);
                break;
            }
            case LuaTableLiteralType:
            {
                sb.Append("table");
                break;
            }
            case LuaDocTableType docTableType:
            {
                RenderLuaDocTableType(docTableType, context, sb, level);
                break;
            }
            case LuaVariadicType variadicType:
            {
                sb.Append("...");
                sb.Append(variadicType.Name);
                break;
            }
            case LuaNamedType namedType:
            {
                RenderNamedType(namedType, context, sb, level);
                break;
            }
            default:
            {
                sb.Append("unknown");
                break;
            }
        }
    }

    private static void RenderNamedType(LuaNamedType namedType, SearchContext context, StringBuilder sb, int level)
    {
        var detailType = namedType.GetDetailType(context);
        if (level == 0)
        {
            if (detailType is AliasDetailType { OriginType: { } originType })
            {
                // ReSharper disable once UselessBinaryOperation
                InnerRenderType(originType, context, sb, level + 1);
            }
        }

        sb.Append(namedType.Name);
    }

    private static void RenderArrayType(LuaArrayType arrayType, SearchContext context, StringBuilder sb, int level)
    {
        InnerRenderType(arrayType.BaseType, context, sb, level + 1);
        sb.Append("[]");
    }

    private static void RenderUnionType(LuaUnionType unionType, SearchContext context, StringBuilder sb, int level)
    {
        if (unionType.UnionTypes.Count == 2 && unionType.UnionTypes.Contains(Builtin.Nil))
        {
            var newType = unionType.Remove(Builtin.Nil);
            InnerRenderType(newType, context, sb, level + 1);
            sb.Append('?');
            return;
        }

        var count = 0;
        foreach (var luaType in unionType.UnionTypes)
        {
            if (count > 0)
            {
                sb.Append('|');
            }

            InnerRenderType(luaType, context, sb, level + 1);
            count++;
        }
    }

    private static void RenderTupleType(LuaTupleType tupleType, SearchContext context, StringBuilder sb, int level)
    {
        sb.Append('[');
        for (var i = 0; i < tupleType.TupleDeclaration.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            InnerRenderType(tupleType.TupleDeclaration[i].DeclarationType!, context, sb, level + 1);
        }

        sb.Append(']');
    }

    private static void RenderFunctionType(LuaMethodType methodType, SearchContext context, StringBuilder sb, int level,
        bool skipFun)
    {
        if (!skipFun)
        {
            sb.Append("fun");
        }

        // show generic
        if (methodType is LuaGenericMethodType genericMethodType)
        {
            var genericParameters = genericMethodType.GenericParameterDeclarations;
            sb.Append('<');
            for (var i = 0; i < genericParameters.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }

                sb.Append(genericParameters[i].Name);
            }

            sb.Append('>');
        }

        var mainSignature = methodType.MainSignature;

        sb.Append('(');
        for (var i = 0; i < mainSignature.Parameters.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(", ");
            }

            var parameter = mainSignature.Parameters[i];
            sb.Append(parameter.Name);
            sb.Append(':');
            InnerRenderType(parameter.DeclarationType ?? Builtin.Any, context, sb, 0);
        }

        sb.Append(')');

        sb.Append(" -> ");
        InnerRenderType(mainSignature.ReturnType, context, sb, 0);
    }

    private static void RenderMultiReturnType(LuaMultiReturnType multiReturnType, SearchContext context,
        StringBuilder sb, int level)
    {
        sb.Append('(');
        for (var i = 0; i < multiReturnType.RetTypes.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            InnerRenderType(multiReturnType.RetTypes[i], context, sb, level + 1);
        }

        sb.Append(')');
    }

    private static void RenderGeneric(LuaGenericType genericType, SearchContext context, StringBuilder sb, int level)
    {
        sb.Append(genericType.Name);
        sb.Append('<');
        for (var i = 0; i < genericType.GenericArgs.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            InnerRenderType(genericType.GenericArgs[i], context, sb, level + 1);
        }

        sb.Append('>');
    }

    private static void RenderLuaDocTableType( LuaDocTableType docTableType, SearchContext context, StringBuilder sb, int level)
    {
        sb.Append('{');
        if (level > 1)
        {
            sb.Append("...}");
            return;
        }
        if (docTableType.DocTablePtr.ToNode(context) is { Body: { } body })
        {
            var fieldList = body.FieldList.ToList();
            for (var i = 0; i < fieldList.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                var field = fieldList[i];
                switch (field)
                {
                    case { NameField: { } nameField, Type: { } type1 }:
                    {
                        var type = context.Infer(type1);
                        sb.Append($"{nameField.RepresentText}:{RenderType(type, context)}");
                        break;
                    }
                    case { IntegerField: { } integerField, Type: { } type2 }:
                    {
                        var type = context.Infer(type2);
                        sb.Append($"[{integerField.Value}]:{RenderType(type, context)}");
                        break;
                    }
                    case { StringField: { } stringField, Type: { } type3 }:
                    {
                        var type = context.Infer(type3);
                        sb.Append($"[{stringField.Value}]:{RenderType(type, context)}");
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

        sb.Append('}');
    }
}
