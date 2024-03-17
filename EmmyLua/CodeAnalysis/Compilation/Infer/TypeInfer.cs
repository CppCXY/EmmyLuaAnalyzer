using System.Diagnostics;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public static class TypeInfer
{
    public static LuaType InferType(LuaDocTypeSyntax type, SearchContext context)
    {
        switch (type)
        {
            case LuaDocTableTypeSyntax tableType:
                return InferTableType(tableType, context);
            case LuaDocArrayTypeSyntax arrayType:
                return InferArrayType(arrayType, context);
            case LuaDocUnionTypeSyntax unionType:
                return InferUnionType(unionType, context);
            case LuaDocLiteralTypeSyntax literalType:
                return InferLiteralType(literalType, context);
            case LuaDocFuncTypeSyntax funcType:
                return InferFuncType(funcType, context);
            case LuaDocNameTypeSyntax nameType:
                return InferNameType(nameType, context);
            case LuaDocParenTypeSyntax parenType:
                return InferParenType(parenType, context);
            case LuaDocTupleTypeSyntax tupleType:
                return InferTupleType(tupleType, context);
            case LuaDocGenericTypeSyntax genericType:
                return InferGenericType(genericType, context);
            default:
                throw new UnreachableException();
        }
    }

    private static LuaType InferTableType(LuaDocTableTypeSyntax tableType, SearchContext context)
    {
        var className = tableType.UniqueId;
        return new LuaNamedType(className);
    }

    private static LuaType InferArrayType(LuaDocArrayTypeSyntax arrayType, SearchContext context)
    {
        var baseTy = context.Infer(arrayType.BaseType);
        return new LuaArrayType(baseTy);
    }

    private static LuaType InferUnionType(LuaDocUnionTypeSyntax unionType, SearchContext context)
    {
        var types = unionType.UnionTypes.Select(context.Infer);
        return new LuaUnionType(types.ToList());
    }

    private static LuaType InferLiteralType(LuaDocLiteralTypeSyntax literalType, SearchContext context)
    {
        if (literalType.Integer != null)
        {
            return new LuaIntegerLiteralType(literalType.Integer.Value);
        }
        else if (literalType.String != null)
        {
            return new LuaStringLiteralType(literalType.String.Value);
        }
        else
        {
            return Builtin.Unknown;
        }
    }

    public static LuaType InferFuncType(LuaDocFuncTypeSyntax funcType, SearchContext context)
    {
        var typedParameters = new List<ParameterLuaDeclaration>();
        foreach (var typedParam in funcType.ParamList)
        {
            if (typedParam is { Name: { } name })
            {
                var paramDeclaration = new ParameterLuaDeclaration(
                    name.RepresentText, name.Position, name,context.Infer(typedParam.Type));
                typedParameters.Add(paramDeclaration);
            }
            else if (typedParam is { VarArgs: { } varArgs })
            {
                var paramDeclaration = new ParameterLuaDeclaration(
                    "...", varArgs.Position, varArgs,context.Infer(typedParam.Type));
                typedParameters.Add(paramDeclaration);
            }
        }

        var returnTypes = funcType.ReturnType.Select(context.Infer).ToList();
        LuaType returnType = Builtin.Unknown;
        if (returnTypes.Count == 1)
        {
            returnType = returnTypes[0];
        }
        else if (returnTypes.Count > 1)
        {
            returnType = new LuaMultiReturnType(returnTypes);
        }

        return new LuaMethodType(returnType, typedParameters, false);
    }

    private static LuaType InferNameType(LuaDocNameTypeSyntax nameType, SearchContext context)
    {
        if (nameType.Name is { RepresentText: {} name} )
        {
            var builtInType = Builtin.FromName(name);
            if (builtInType is not null)
            {
                return builtInType;
            }

            return new LuaNamedType(name);
        }

        return Builtin.Unknown;
    }

    private static LuaType InferParenType(LuaDocParenTypeSyntax parenType, SearchContext context)
    {
        return parenType.Type != null
            ? InferType(parenType.Type, context)
            : Builtin.Unknown;
    }

    private static LuaType InferTupleType(LuaDocTupleTypeSyntax tupleType, SearchContext context)
    {
        var types = tupleType.TypeList.Select(context.Infer).ToList();
        return new LuaTupleType(types);
    }

    private static LuaType InferGenericType(LuaDocGenericTypeSyntax genericType, SearchContext context)
    {
        var typeArgs = genericType.GenericArgs.Select(context.Infer).ToList();
        if (genericType is { Name: { } name })
        {
            return new LuaGenericType(name.RepresentText, typeArgs);
        }

        return Builtin.Unknown;
    }
}
