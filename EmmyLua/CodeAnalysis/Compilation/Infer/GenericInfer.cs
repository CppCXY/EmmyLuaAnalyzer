using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public static class GenericInfer
{
    public static void InferInstantiateByExpr(
        LuaType type,
        LuaExprSyntax expr,
        Dictionary<string, LuaType> genericParameter,
        Dictionary<string, LuaType> genericReplace,
        SearchContext context)
    {
        if (type is LuaTemplateType templateType && expr is LuaLiteralExprSyntax {Literal: LuaStringToken {} stringToken})
        {
            TemplateTypeInstantiateByString(templateType, stringToken, genericParameter, genericReplace, context);
            return;
        }

        var exprType = context.Infer(expr);
        InferInstantiateByType(type, exprType, genericParameter, genericReplace, context);
    }

    public static void InferInstantiateByExpandTypeAndExprs(
        LuaExpandType expandType,
        IEnumerable<LuaExprSyntax> expr,
        Dictionary<string, LuaType> genericParameter,
        Dictionary<string, LuaType> genericReplace,
        SearchContext context
    )
    {
        InferInstantiateByExpandTypeAndTypes(expandType, expr.Select(context.Infer), genericParameter, genericReplace,
            context);
    }

    public static void InferInstantiateByExpandTypeAndTypes(
        LuaExpandType expandType,
        IEnumerable<LuaType> types,
        Dictionary<string, LuaType> genericParameter,
        Dictionary<string, LuaType> genericReplace,
        SearchContext context
    )
    {
        if (genericParameter.ContainsKey(expandType.Name))
        {
            genericReplace.TryAdd(expandType.Name, new LuaMultiReturnType(types.ToList()));
        }
    }

    public static void InferInstantiateByType(
        LuaType type,
        LuaType exprType,
        Dictionary<string, LuaType> genericParameter,
        Dictionary<string, LuaType> genericReplace,
        SearchContext context)
    {
        switch (type)
        {
            case LuaGenericType genericType:
            {
                GenericInstantiateByType(genericType, exprType, genericParameter, genericReplace, context);
                break;
            }
            case LuaNamedType namedType:
            {
                NamedTypeInstantiateByType(namedType, exprType, genericParameter, genericReplace, context);
                break;
            }
            case LuaArrayType arrayType:
            {
                ArrayTypeInstantiateByType(arrayType, exprType, genericParameter, genericReplace, context);
                break;
            }
            case LuaMethodType methodType:
            {
                MethodTypeInstantiateByType(methodType, exprType, genericParameter, genericReplace, context);
                break;
            }
            case LuaUnionType unionType:
            {
                UnionTypeInstantiateByType(unionType, exprType, genericParameter, genericReplace, context);
                break;
            }
            case LuaTupleType tupleType:
            {
                TupleTypeInstantiateByType(tupleType, exprType, genericParameter, genericReplace, context);
                break;
            }
        }
    }

    private static bool IsGenericParameter(string name, Dictionary<string, LuaType> genericParameter)
    {
        return genericParameter.ContainsKey(name);
    }

    private static void GenericInstantiateByType(
        LuaGenericType genericType,
        LuaType exprType,
        Dictionary<string, LuaType> genericParameter,
        Dictionary<string, LuaType> genericReplace,
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
                    InferInstantiateByType(genericArgs1[i], genericArgs2[i], genericParameter, genericReplace, context);
                }
            }
        }
        else if (exprType is LuaTableLiteralType tableType)
        {
            if (IsGenericParameter(genericType.Name, genericParameter))
            {
                genericReplace.TryAdd(genericType.Name, Builtin.Table);
            }

            var tableExpr = tableType.TableExprPtr.ToNode(context);
            if (tableExpr is not null)
            {
                GenericTableExprInstantiate(genericType, tableExpr, genericParameter, genericReplace, context);
            }
        }
    }

    private static void GenericTableExprInstantiate(
        LuaGenericType genericType,
        LuaTableExprSyntax tableExpr,
        Dictionary<string, LuaType> genericParameter,
        Dictionary<string, LuaType> genericReplace,
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

        InferInstantiateByType(genericArgs[0], keyType, genericParameter, genericReplace, context);
        InferInstantiateByType(genericArgs[1], valueType, genericParameter, genericReplace, context);
    }

    private static void NamedTypeInstantiateByType(
        LuaNamedType namedType,
        LuaType exprType,
        Dictionary<string, LuaType> genericParameter,
        Dictionary<string, LuaType> genericReplace,
        SearchContext context)
    {
        if (IsGenericParameter(namedType.Name, genericParameter))
        {
            genericReplace.TryAdd(namedType.Name, exprType);
        }
    }

    private static void ArrayTypeInstantiateByType(
        LuaArrayType arrayType,
        LuaType exprType,
        Dictionary<string, LuaType> genericParameter,
        Dictionary<string, LuaType> genericReplace,
        SearchContext context)
    {
        if (exprType is LuaArrayType arrayType2)
        {
            InferInstantiateByType(arrayType.BaseType, arrayType2.BaseType, genericParameter, genericReplace, context);
        }
        else if (exprType is LuaTableLiteralType tableLiteralType)
        {
            var tableExpr = tableLiteralType.TableExprPtr.ToNode(context);
            if (tableExpr is not null)
            {
                LuaType valueType = Builtin.Unknown;

                foreach (var field in tableExpr.FieldList)
                {
                    if (field.IsValue)
                    {
                        var fieldValueType = context.Infer(field.Value);
                        valueType = valueType.Union(fieldValueType);
                    }
                }

                InferInstantiateByType(arrayType.BaseType, valueType, genericParameter, genericReplace, context);
            }
        }
    }

    private static void MethodTypeInstantiateByType(
        LuaMethodType methodType,
        LuaType exprType,
        Dictionary<string, LuaType> genericParameter,
        Dictionary<string, LuaType> genericReplace,
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
                InferInstantiateByType(parameter.Type, parameter2.Type, genericParameter, genericReplace, context);
            }

            if (mainSignature.ReturnType is { } returnType && mainSignature2.ReturnType is { } returnType2)
            {
                InferInstantiateByType(returnType, returnType2, genericParameter, genericReplace, context);
            }
        }
    }

    private static void UnionTypeInstantiateByType(
        LuaUnionType unionType,
        LuaType exprType,
        Dictionary<string, LuaType> genericParameter,
        Dictionary<string, LuaType> genericReplace,
        SearchContext context)
    {
        if (unionType.UnionTypes.Contains(Builtin.Nil))
        {
            var newType = unionType.Remove(Builtin.Nil);
            InferInstantiateByType(newType, exprType, genericParameter, genericReplace, context);
        }

        foreach (var luaType in unionType.UnionTypes)
        {
            InferInstantiateByType(luaType, exprType, genericParameter, genericReplace, context);
            if (genericParameter.Count == genericReplace.Count)
            {
                break;
            }
        }
    }

    private static void TupleTypeInstantiateByType(
        LuaTupleType tupleType,
        LuaType exprType,
        Dictionary<string, LuaType> genericParameter,
        Dictionary<string, LuaType> genericReplace,
        SearchContext context)
    {
        if (exprType is LuaTupleType tupleType2)
        {
            for (var i = 0; i < tupleType.TupleDeclaration.Count && i < tupleType2.TupleDeclaration.Count; i++)
            {
                var leftElementType = tupleType.TupleDeclaration[i].Type;
                if (leftElementType is LuaExpandType expandType)
                {
                    var rightExprs = tupleType2.TupleDeclaration[i..]
                        .Select(it => it.Type);
                    InferInstantiateByExpandTypeAndTypes(expandType, rightExprs, genericParameter, genericReplace,
                        context);
                }
                else
                {
                    var rightElementType = tupleType2.TupleDeclaration[i].Type;
                    InferInstantiateByType(leftElementType, rightElementType, genericParameter, genericReplace, context);
                }
            }
        }
        else if (exprType is LuaTableLiteralType tableLiteralType)
        {
            var tableExpr = tableLiteralType.TableExprPtr.ToNode(context);
            if (tableExpr is not null)
            {
                var fileList = tableExpr.FieldList.ToList();
                for (var i = 0; i < fileList.Count && i < tupleType.TupleDeclaration.Count; i++)
                {
                    var tupleElementType = tupleType.TupleDeclaration[i].Type;
                    if (tupleElementType is LuaExpandType expandType)
                    {
                        var fileExprs = fileList[i..]
                            .Where(it => it is { IsValue: true, Value: not null })
                            .Select(it => it.Value!);
                        InferInstantiateByExpandTypeAndExprs(expandType, fileExprs, genericParameter, genericReplace,
                            context);
                        break;
                    }
                    else
                    {
                        var field = fileList[i];
                        if (field is { IsValue: true, Value: { } valueExpr })
                        {
                            InferInstantiateByExpr(
                                tupleElementType,
                                valueExpr,
                                genericParameter,
                                genericReplace, context);
                        }
                    }
                }
            }
        }
    }

    private static void TemplateTypeInstantiateByString(
        LuaTemplateType templateType,
        LuaStringToken stringToken,
        Dictionary<string, LuaType> genericParameter,
        Dictionary<string, LuaType> genericReplace,
        SearchContext context)
    {
        if (IsGenericParameter(templateType.TemplateName, genericParameter))
        {
            genericReplace.TryAdd(templateType.TemplateName, new LuaNamedType(stringToken.Value));
        }
    }
}
