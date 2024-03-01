using System.Diagnostics;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
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
        var className = context.GetUniqueId(tableType);
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
            return context.Compilation.Builtin.Unknown;
        }
    }

    public static LuaType InferFuncType(LuaDocFuncTypeSyntax funcType, SearchContext context)
    {
        var typedParameters = new List<TypedParameter>();
        foreach (var typedParam in funcType.ParamList)
        {
            if (typedParam is { Name: { } name })
            {
                 typedParameters.Add(new TypedParameter(name.RepresentText, context.Infer(typedParam.Type)));
            }
            else if (typedParam is { VarArgs: { } varArgs })
            {
                typedParameters.Add(new TypedParameter(varArgs.RepresentText, context.Infer(typedParam.Type)));
            }
        }

        var returnTypes = funcType.ReturnType.Select(context.Infer).ToList();
        return new LuaMethodType(new LuaReturnType(returnTypes), typedParameters);
    }

    private static LuaType InferNameType(LuaDocNameTypeSyntax nameType, SearchContext context)
    {
        return nameType.Name != null
            ? new LuaNamedType(nameType.Name.RepresentText)
            : context.Compilation.Builtin.Unknown;
    }

    private static LuaType InferParenType(LuaDocParenTypeSyntax parenType, SearchContext context)
    {
        return parenType.Type != null
            ? InferType(parenType.Type, context)
            : context.Compilation.Builtin.Unknown;
    }

    private static LuaType InferTupleType(LuaDocTupleTypeSyntax tupleType, SearchContext context)
    {
        var types = tupleType.TypeList.Select(context.Infer).ToList();
        return new LuaTupleType(types);
    }

    private static LuaType InferGenericType(LuaDocGenericTypeSyntax genericType, SearchContext context)
    {
        var typeArgs = genericType.GenericArgs.Select(context.Infer).ToList();
        if (genericType is { Name.Name: { } name })
        {
            return new LuaGenericType(name.RepresentText, typeArgs);
        }

        return context.Compilation.Builtin.Unknown;
    }
}
