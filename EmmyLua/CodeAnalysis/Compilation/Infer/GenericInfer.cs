using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public static class GenericInfer
{
    public static void InferInstantiateByExpr(
        LuaType type,
        LuaExprSyntax expr,
        HashSet<string> genericParameter,
        Dictionary<string, LuaType> result,
        SearchContext context)
    {
        switch (type)
        {
            case LuaGenericType genericType:
            {
                GenericInstantiateByExpr(genericType, expr, genericParameter, result, context);
                break;
            }
            case LuaNamedType namedType:
            {
                NamedTypeInstantiateByExpr(namedType, expr, genericParameter, result, context);
                break;
            }
            case LuaArrayType arrayType:
            {
                ArrayTypeInstantiateByExpr(arrayType, expr, genericParameter, result, context);
                break;
            }
            case LuaMethodType methodType:
            {
                MethodTypeInstantiateByExpr(methodType, expr, genericParameter, result, context);
                break;
            }
            case LuaUnionType unionType:
            {
                UnionTypeInstantiateByExpr(unionType, expr, genericParameter, result, context);
                break;
            }
            case LuaTupleType tupleType:
            {
                TupleTypeInstantiateByExpr(tupleType, expr, genericParameter, result, context);
                break;
            }
        }
    }

    public static void InferInstantiateByType(
        LuaType type,
        LuaType exprType,
        HashSet<string> genericParameter,
        Dictionary<string, LuaType> result,
        SearchContext context)
    {
        switch (type)
        {
            case LuaGenericType genericType:
            {
                GenericInstantiateByType(genericType, exprType, genericParameter, result, context);
                break;
            }
            case LuaNamedType namedType:
            {
                NamedTypeInstantiateByType(namedType, exprType, genericParameter, result, context);
                break;
            }
            case LuaArrayType arrayType:
            {
                ArrayTypeInstantiateByType(arrayType, exprType, genericParameter, result, context);
                break;
            }
            case LuaMethodType methodType:
            {
                MethodTypeInstantiateByType(methodType, exprType, genericParameter, result, context);
                break;
            }
            case LuaUnionType unionType:
            {
                UnionTypeInstantiateByType(unionType, exprType, genericParameter, result, context);
                break;
            }
            case LuaTupleType tupleType:
            {
                TupleTypeInstantiateByType(tupleType, exprType, genericParameter, result, context);
                break;
            }
        }
    }

    private static bool IsGenericParameter(string name, HashSet<string> genericParameter)
    {
        return genericParameter.Contains(name);
    }

    private static void GenericInstantiateByExpr(LuaGenericType genericType,
        LuaExprSyntax expr,
        HashSet<string> genericParameter,
        Dictionary<string, LuaType> result,
        SearchContext context)
    {
        if (expr is LuaTableExprSyntax table)
        {
            if (IsGenericParameter(genericType.Name, genericParameter))
            {
                result.TryAdd(genericType.Name, Builtin.Table);
            }

            GenericTableTypeInstantiate(genericType, table, genericParameter, result, context);
        }
    }

    private static void GenericInstantiateByType(LuaGenericType genericType,
        LuaType exprType,
        HashSet<string> genericParameter,
        Dictionary<string, LuaType> result,
        SearchContext context)
    {
        if (exprType is LuaGenericType genericType2)
        {
            if (genericType2.Name == genericType.Name)
            {
                var genericArgs1 = genericType.GenericArgs;
                var genericArgs2 = genericType2.GenericArgs;

                for (int i = 0; i < genericArgs1.Count && i < genericArgs2.Count; i++)
                {
                    InferInstantiateByType(genericArgs1[i], genericArgs2[i], genericParameter, result, context);
                }
            }
        }
    }

    private static void GenericTableTypeInstantiate(LuaGenericType genericType,
        LuaTableExprSyntax tableExpr,
        HashSet<string> genericParameter,
        Dictionary<string, LuaType> result,
        SearchContext context)
    {
        var genericArgs = genericType.GenericArgs;
        if (genericArgs.Count != 2)
        {
            return;
        }

        LuaType keyType = Builtin.Unknown;
        LuaType valueType = Builtin.Unknown;

        foreach (var fieldSyntax in tableExpr.FieldList)
        {
            if (fieldSyntax.IsValue)
            {
                keyType = keyType.Union(Builtin.Integer);
            }
            else if (fieldSyntax.IsStringKey || fieldSyntax.IsNameKey)
            {
                keyType = keyType.Union(Builtin.String);
            }

            var fieldValueType = context.Infer(fieldSyntax.Value);
            valueType = valueType.Union(fieldValueType);
        }

        InferInstantiateByType(genericArgs[0], keyType, genericParameter, result, context);
        InferInstantiateByType(genericArgs[1], valueType, genericParameter, result, context);
    }

    private static void NamedTypeInstantiateByExpr(LuaNamedType namedType,
        LuaExprSyntax expr,
        HashSet<string> genericParameter,
        Dictionary<string, LuaType> result,
        SearchContext context)
    {
        var exprType = context.Infer(expr);
        InferInstantiateByType(namedType, exprType, genericParameter, result, context);
    }

    private static void NamedTypeInstantiateByType(LuaNamedType namedType,
        LuaType exprType,
        HashSet<string> genericParameter,
        Dictionary<string, LuaType> result,
        SearchContext context)
    {
        if (IsGenericParameter(namedType.Name, genericParameter))
        {
            result.TryAdd(namedType.Name, exprType);
        }
    }

    private static void ArrayTypeInstantiateByExpr(LuaArrayType arrayType,
        LuaExprSyntax expr,
        HashSet<string> genericParameter,
        Dictionary<string, LuaType> result,
        SearchContext context)
    {
        if (expr is LuaTableExprSyntax tableExpr)
        {
            LuaType valueType = Builtin.Unknown;

            foreach (var fieldSyntax in tableExpr.FieldList)
            {
                if (fieldSyntax.IsValue)
                {
                    var fieldValueType = context.Infer(fieldSyntax.Value);
                    valueType = valueType.Union(fieldValueType);
                }
            }

            InferInstantiateByType(arrayType.BaseType, valueType, genericParameter, result, context);
        }
        else
        {
            var exprType = context.Infer(expr);
            InferInstantiateByType(arrayType, exprType, genericParameter, result, context);
        }
    }

    private static void ArrayTypeInstantiateByType(LuaArrayType arrayType,
        LuaType exprType,
        HashSet<string> genericParameter,
        Dictionary<string, LuaType> result,
        SearchContext context)
    {
        if (exprType is LuaArrayType arrayType2)
        {
            InferInstantiateByType(arrayType.BaseType, arrayType2.BaseType, genericParameter, result, context);
        }
    }

    private static void MethodTypeInstantiateByExpr(LuaMethodType methodType,
        LuaExprSyntax expr,
        HashSet<string> genericParameter,
        Dictionary<string, LuaType> result,
        SearchContext context)
    {
        var exprType = context.Infer(expr);
        InferInstantiateByType(methodType, exprType, genericParameter, result, context);
    }

    private static void MethodTypeInstantiateByType(LuaMethodType methodType,
        LuaType exprType,
        HashSet<string> genericParameter,
        Dictionary<string, LuaType> result,
        SearchContext context)
    {
        if (exprType is LuaMethodType methodType2)
        {
            var mainSignature = methodType.MainSignature;
            var mainSignature2 = methodType2.MainSignature;
            for (int i = 0; i < mainSignature.Parameters.Count && i < mainSignature2.Parameters.Count; i++)
            {
                var parameter = mainSignature.Parameters[i];
                var parameter2 = mainSignature2.Parameters[i];
                if (parameter is { DeclarationType: { } type })
                {
                    var paramType = parameter2.DeclarationType ?? Builtin.Any;
                    InferInstantiateByType(type, paramType, genericParameter, result, context);
                }
            }

            if (mainSignature.ReturnType is { } returnType && mainSignature2.ReturnType is { } returnType2)
            {
                InferInstantiateByType(returnType, returnType2, genericParameter, result, context);
            }
        }
    }

    private static void UnionTypeInstantiateByExpr(LuaUnionType unionType,
        LuaExprSyntax expr,
        HashSet<string> genericParameter,
        Dictionary<string, LuaType> result,
        SearchContext context)
    {
        if (unionType.UnionTypes.Contains(Builtin.Nil))
        {
            var newType = unionType.Remove(Builtin.Nil);
            InferInstantiateByExpr(newType, expr, genericParameter, result, context);
        }
        // TODO: other cases
    }

    private static void UnionTypeInstantiateByType(LuaUnionType unionType,
        LuaType exprType,
        HashSet<string> genericParameter,
        Dictionary<string, LuaType> result,
        SearchContext context)
    {
        // TODO: Not implemented
    }

    private static void TupleTypeInstantiateByExpr(LuaTupleType tupleType,
        LuaExprSyntax expr,
        HashSet<string> genericParameter,
        Dictionary<string, LuaType> result,
        SearchContext context)
    {
        if (expr is LuaTableExprSyntax tableExpr)
        {
            var arrayCount = 0;
            foreach (var fieldSyntax in tableExpr.FieldList)
            {
                if (fieldSyntax.IsValue)
                {
                    if (arrayCount >= tupleType.TupleTypes.Count)
                    {
                        break;
                    }

                    if (fieldSyntax.Value is not null)
                    {
                        InferInstantiateByExpr(tupleType.TupleTypes[arrayCount], fieldSyntax.Value, genericParameter,
                            result, context);
                    }

                    arrayCount++;
                }
            }
        }
        else
        {
            var exprType = context.Infer(expr);
            InferInstantiateByType(tupleType, exprType, genericParameter, result, context);
        }
    }

    private static void TupleTypeInstantiateByType(LuaTupleType tupleType,
        LuaType exprType,
        HashSet<string> genericParameter,
        Dictionary<string, LuaType> result,
        SearchContext context)
    {
        if (exprType is LuaTupleType tupleType2)
        {
            for (var i = 0; i < tupleType.TupleTypes.Count && i < tupleType2.TupleTypes.Count; i++)
            {
                InferInstantiateByType(tupleType.TupleTypes[i], tupleType2.TupleTypes[i], genericParameter, result, context);
            }
        }
    }
}
