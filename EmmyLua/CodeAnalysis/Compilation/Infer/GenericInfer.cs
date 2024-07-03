using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public static class GenericInfer
{
    public static void InferByExpr(
        LuaType type,
        LuaExprSyntax expr,
        TypeSubstitution substitution,
        SearchContext context)
    {
        if (SpecialInferByExpr(type, expr, substitution, context))
        {
            return;
        }

        var exprType = context.InferAndUnwrap(expr);
        InferByType(type, exprType, substitution, context);
    }

    private static bool SpecialInferByExpr(
        LuaType type,
        LuaExprSyntax expr,
        TypeSubstitution substitution,
        SearchContext context)
    {
        if (type is LuaTemplateType templateType && expr is LuaLiteralExprSyntax
            {
                Literal: LuaStringToken { } stringToken1
            })
        {
            substitution.Add(templateType.TemplateName, new LuaNamedType(templateType.PrefixName + stringToken1.Value));
            return true;
        }

        if (type is LuaGenericType { Name: "strFmt" } genericType && expr is LuaLiteralExprSyntax
            {
                Literal: LuaStringToken { } stringToken2
            })
        {
            StrFmtInstantiateByString(genericType, stringToken2.Value, substitution, context);
            return true;
        }

        return false;
    }

    public static void InferByExpandTypeAndExprs(
        LuaExpandType expandType,
        IEnumerable<LuaExprSyntax> expr,
        TypeSubstitution substitution,
        SearchContext context
    )
    {
        substitution.Add(expandType.Name, new LuaMultiReturnType(expr.Select(context.InferAndUnwrap).ToList()));
    }

    public static void InferByType(
        LuaType leftType,
        LuaType rightType,
        TypeSubstitution substitution,
        SearchContext context)
    {
        switch (leftType)
        {
            case LuaGenericType genericType:
            {
                GenericInstantiateByType(genericType, rightType, substitution, context);
                break;
            }
            case LuaNamedType namedType:
            {
                substitution.Add(namedType.Name, rightType);
                break;
            }
            case LuaArrayType arrayType:
            {
                ArrayTypeInstantiateByType(arrayType, rightType, substitution, context);
                break;
            }
            case LuaMethodType methodType:
            {
                MethodTypeInstantiateByType(methodType, rightType, substitution, context);
                break;
            }
            case LuaUnionType unionType:
            {
                UnionTypeInstantiateByType(unionType, rightType, substitution, context);
                break;
            }
            case LuaTupleType tupleType:
            {
                TupleTypeInstantiateByType(tupleType, rightType, substitution, context);
                break;
            }
            case LuaExpandType expandType:
            {
                substitution.Add(expandType.Name, rightType);
                break;
            }
        }
    }

    private static void GenericInstantiateByType(
        LuaGenericType genericType,
        LuaType exprType,
        TypeSubstitution substitution,
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
                    InferByType(genericArgs1[i], genericArgs2[i], substitution, context);
                }
            }
        }
        else if (exprType is LuaTableLiteralType tableType)
        {
            if (substitution.IsGenericParam(genericType.Name))
            {
                substitution.Add(genericType.Name, Builtin.Table);
            }

            var tableExpr = tableType.TableExprPtr.ToNode(context);
            if (tableExpr is not null)
            {
                GenericTableExprInstantiate(genericType, tableExpr, substitution, context);
            }
        }
    }

    private static void GenericTableExprInstantiate(
        LuaGenericType genericType,
        LuaTableExprSyntax tableExpr,
        TypeSubstitution substitution,
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
            if (valueType is LuaUnionType { UnionTypes.Count: > 2 })
            {
                break;
            }

            if (fieldSyntax.IsValue)
            {
                keyType = keyType.Union(Builtin.Integer);
            }
            else if (fieldSyntax.IsStringKey || fieldSyntax.IsNameKey)
            {
                keyType = keyType.Union(Builtin.String);
            }
            var fieldValueType = context.InferAndUnwrap(fieldSyntax.Value);
            valueType = valueType.Union(fieldValueType);
        }

        InferByType(genericArgs[0], keyType, substitution, context);
        InferByType(genericArgs[1], valueType, substitution, context);
    }

    private static void ArrayTypeInstantiateByType(
        LuaArrayType arrayType,
        LuaType exprType,
        TypeSubstitution substitution,
        SearchContext context)
    {
        if (exprType is LuaArrayType arrayType2)
        {
            InferByType(arrayType.BaseType, arrayType2.BaseType, substitution, context);
        }
        else if (exprType is LuaTableLiteralType tableLiteralType)
        {
            var tableExpr = tableLiteralType.TableExprPtr.ToNode(context);
            if (tableExpr is not null)
            {
                LuaType valueType = Builtin.Unknown;

                foreach (var field in tableExpr.FieldList)
                {
                    if (valueType is LuaUnionType { UnionTypes.Count: > 2 })
                    {
                        break;
                    }

                    if (field.IsValue)
                    {
                        var fieldValueType = context.InferAndUnwrap(field.Value);
                        valueType = valueType.Union(fieldValueType);
                    }
                }

                InferByType(arrayType.BaseType, valueType, substitution, context);
            }
        }
    }

    private static void MethodTypeInstantiateByType(
        LuaMethodType methodType,
        LuaType exprType,
        TypeSubstitution substitution,
        SearchContext context)
    {
        if (exprType is LuaMethodType methodType2)
        {
            var mainSignature = methodType.MainSignature;
            var mainSignature2 = methodType2.MainSignature;
            for (var i = 0; i < mainSignature.Parameters.Count && i < mainSignature2.Parameters.Count; i++)
            {
                var leftParamType = mainSignature.Parameters[i].Type;
                if (leftParamType is LuaExpandType expandType)
                {
                    substitution.AddSpreadParameter(expandType.Name, mainSignature2.Parameters[i..]);
                    break;
                }
                else
                {
                    var rightParamType = mainSignature2.Parameters[i].Type;
                    InferByType(leftParamType, rightParamType, substitution, context);
                }
            }

            if (mainSignature.ReturnType is { } returnType && mainSignature2.ReturnType is { } returnType2)
            {
                InferByType(returnType, returnType2, substitution, context);
            }
        }
    }

    private static void UnionTypeInstantiateByType(
        LuaUnionType unionType,
        LuaType exprType,
        TypeSubstitution substitution,
        SearchContext context)
    {
        if (unionType.UnionTypes.Contains(Builtin.Nil))
        {
            var newType = unionType.Remove(Builtin.Nil);
            InferByType(newType, exprType, substitution, context);
        }

        foreach (var luaType in unionType.UnionTypes)
        {
            InferByType(luaType, exprType, substitution, context);
            if (substitution.InferFinished)
            {
                break;
            }
        }
    }

    private static void TupleTypeInstantiateByType(
        LuaTupleType tupleType,
        LuaType exprType,
        TypeSubstitution substitution,
        SearchContext context)
    {
        if (exprType is LuaTupleType tupleType2)
        {
            for (var i = 0; i < tupleType.TupleDeclaration.Count && i < tupleType2.TupleDeclaration.Count; i++)
            {
                var leftElementType = tupleType.TupleDeclaration[i].Type;
                if (leftElementType is LuaExpandType expandType)
                {
                    var rightExprTypes = tupleType2.TupleDeclaration[i..]
                        .Select(it => it.Type);
                    substitution.Add(expandType.Name, new LuaMultiReturnType(rightExprTypes.ToList()));
                }
                else
                {
                    var rightElementType = tupleType2.TupleDeclaration[i].Type;
                    InferByType(leftElementType, rightElementType, substitution,
                        context);
                }
            }
        }
        else if (exprType is LuaArrayType arrayType)
        {
            var arrayElementType = arrayType.BaseType;
            foreach (var tupleElement in tupleType.TupleDeclaration)
            {
                var leftElementType = tupleElement.Type;
                if (leftElementType is LuaExpandType expandType)
                {
                    substitution.Add(expandType.Name, new LuaMultiReturnType(arrayElementType));
                    break;
                }
                else
                {
                    InferByType(leftElementType, arrayElementType, substitution,
                        context);
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
                        InferByExpandTypeAndExprs(expandType, fileExprs, substitution,
                            context);
                        break;
                    }
                    else
                    {
                        var field = fileList[i];
                        if (field is { IsValue: true, Value: { } valueExpr })
                        {
                            InferByExpr(
                                tupleElementType,
                                valueExpr,
                                substitution,
                                context);
                        }
                    }
                }
            }
        }
    }

    private static void StrFmtInstantiateByString(
        LuaGenericType genericType,
        string fmt,
        TypeSubstitution substitution,
        SearchContext context)
    {
        var firstTemp = genericType.GenericArgs.FirstOrDefault();
        if (firstTemp is not LuaNamedType templateName)
        {
            return;
        }

        var spreadParameter = new List<IDeclaration>();
        for(var i = 0; i < fmt.Length; i++)
        {
            var ch = fmt[i];
            if (fmt[i] == '%')
            {
                if (i + 1 < fmt.Length)
                {
                    var nextCh = fmt[i + 1];
                    if (nextCh == '%')
                    {
                        i++;
                    }
                    else
                    {
                        var index = i + 1;
                        while (index < fmt.Length && char.IsDigit(fmt[index]))
                        {
                            index++;
                        }

                        if (index < fmt.Length)
                        {
                            var type = fmt[index];
                            if (type is 's' or 'q')
                            {
                                spreadParameter.Add(new LuaDeclaration(
                                    $"%{type}",
                                    new VirtualInfo(Builtin.Any)
                                    ));
                            }
                            else if (type is 'c' or 'd' or 'i' or 'u' or 'x' or 'X' or 'o')
                            {
                                spreadParameter.Add(new LuaDeclaration(
                                    $"%{type}",
                                    new VirtualInfo(Builtin.Integer)
                                    ));
                            }
                            else if (type is 'A' or 'a' or 'E' or 'e' or 'f' or 'G' or 'g')
                            {
                                spreadParameter.Add(new LuaDeclaration(
                                    $"%{type}",
                                    new VirtualInfo(Builtin.Number)
                                    ));
                            }
                        }
                    }
                }
            }
        }

        substitution.AddSpreadParameter(templateName.Name, spreadParameter);
    }
}
