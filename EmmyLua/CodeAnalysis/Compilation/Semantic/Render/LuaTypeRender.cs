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

        return "()=>void";
    }

    private static void InnerRenderType(LuaType type, SearchContext context, StringBuilder sb, int level)
    {
        switch (type)
        {
            case LuaNamedType namedType:
            {
                RenderNamedType(namedType, context, sb, level);
                break;
            }
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
            default:
            {
                sb.Append("unknown");
                break;
            }
        }
    }

    private static void RenderNamedType(LuaNamedType namedType, SearchContext context, StringBuilder sb, int level)
    {
        switch (namedType)
        {
            case LuaGenericType genericType:
            {
                sb.Append(genericType.Name);
                sb.Append('<');
                for (var i = 0; i < genericType.GenericArgs.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    InnerRenderType(genericType.GenericArgs[i], context, sb, level + 1);
                }

                sb.Append('>');
                break;
            }
            case LuaTableLiteralType:
            {
                sb.Append("table");
                break;
            }
            default:
            {
                if (level == 0)
                {
                    var detailType = namedType.GetDetailType(context);
                    if (detailType is AliasDetailType { OriginType: { } originType })
                    {
                        // ReSharper disable once UselessBinaryOperation
                        InnerRenderType(originType, context, sb, level + 1);
                        break;
                    }
                }

                sb.Append(namedType.Name);
                break;
            }
        }
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
        for (var i = 0; i < tupleType.TupleTypes.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }

            InnerRenderType(tupleType.TupleTypes[i], context, sb, level + 1);
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

        sb.Append("->");
        InnerRenderType(mainSignature.ReturnType, context, sb, 0);
    }

    private static void RenderMultiReturnType(LuaMultiReturnType multiReturnType, SearchContext context, StringBuilder sb, int level)
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
}
