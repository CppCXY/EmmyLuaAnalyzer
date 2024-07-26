using System.Diagnostics;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;

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
            case LuaDocVariadicTypeSyntax genericVarargType:
                return InferVariadicType(genericVarargType, context);
            case LuaDocExpandTypeSyntax expandType:
                return InferExpandType(expandType, context);
            case LuaDocAggregateTypeSyntax aggregateType:
                return InferAggregateType(aggregateType, context);
            case LuaDocTemplateTypeSyntax templateType:
                return InferTemplateType(templateType, context);
            default:
                throw new UnreachableException();
        }
    }

    private static LuaType InferTableType(LuaDocTableTypeSyntax tableType, SearchContext context)
    {
        return new LuaElementType(tableType.UniqueId);
    }

    private static LuaType InferArrayType(LuaDocArrayTypeSyntax arrayType, SearchContext context)
    {
        var baseTy = context.Infer(arrayType.BaseType);
        return new LuaArrayType(baseTy);
    }

    private static LuaType InferUnionType(LuaDocUnionTypeSyntax unionType, SearchContext context)
    {
        var types = unionType.UnionTypes.Select(context.Infer).ToList();
        if (types.Count == 1)
        {
            return types[0];
        }

        return new LuaUnionType(types);
    }

    private static LuaType InferLiteralType(LuaDocLiteralTypeSyntax literalType, SearchContext context)
    {
        if (literalType.Integer != null)
        {
            return new LuaIntegerLiteralType(literalType.Integer.Value);
        }

        if (literalType.String != null)
        {
            return new LuaStringLiteralType(literalType.String.Value);
        }

        return Builtin.Unknown;
    }

    private static LuaType InferFuncType(LuaDocFuncTypeSyntax funcType, SearchContext context)
    {
        var typedParameters = new List<LuaSymbol>();
        foreach (var typedParam in funcType.ParamList)
        {
            if (typedParam is { Name: { } name, Nullable: { } nullable })
            {
                var type = context.Infer(typedParam.Type);
                if (nullable)
                {
                    type = type.Union(Builtin.Nil);
                }

                var paramDeclaration = new LuaSymbol(name.RepresentText,
                    new ParamInfo(new(typedParam), type, false));
                typedParameters.Add(paramDeclaration);
            }
            else if (typedParam is { VarArgs: { } varArgs })
            {
                var paramDeclaration = new LuaSymbol("...",
                    new ParamInfo(new(typedParam), context.Infer(typedParam.Type), true));
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
        if (nameType.Name is { RepresentText: { } name })
        {
            return new LuaNamedType(nameType.DocumentId, name);
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
        var tupleMembers = tupleType.TypeList
            .Select((it, i) =>
                // lua from 1 start
                new LuaSymbol($"[{i + 1}]", new TupleMemberInfo(
                    i + 1, context.Infer(it), new(it)
                ))
            )
            .Cast<LuaSymbol>()
            .ToList();
        return new LuaTupleType(tupleMembers);
    }

    private static LuaType InferGenericType(LuaDocGenericTypeSyntax genericType, SearchContext context)
    {
        var typeArgs = genericType.GenericArgs.Select(context.Infer).ToList();
        if (genericType is { Name: { } name })
        {
            return new LuaGenericType(genericType.DocumentId, name.RepresentText, typeArgs);
        }

        return Builtin.Unknown;
    }

    private static LuaType InferVariadicType(LuaDocVariadicTypeSyntax variadicType, SearchContext context)
    {
        if (variadicType is { Name: { } name })
        {
            return new LuaVariadicType(new LuaNamedType(variadicType.DocumentId, name.RepresentText));
        }

        return new LuaVariadicType(Builtin.Unknown);
    }

    private static LuaType InferExpandType(LuaDocExpandTypeSyntax expandType, SearchContext context)
    {
        if (expandType is { Name: { } name })
        {
            return new LuaExpandType(name.RepresentText);
        }

        return Builtin.Unknown;
    }

    private static LuaType InferAggregateType(LuaDocAggregateTypeSyntax aggregateType, SearchContext context)
    {
        var declarations = aggregateType.TypeList
            .Select((it, i) =>
                new LuaSymbol(string.Empty, new AggregateMemberInfo(
                    new(it), context.Infer(it)
                ))
            )
            .ToList();
        return new LuaAggregateType(declarations);
    }

    private static LuaType InferTemplateType(LuaDocTemplateTypeSyntax templateType, SearchContext context)
    {
        var prefixName = templateType.PrefixName?.RepresentText ?? string.Empty;
        if (templateType.TemplateName?.Name is { } name)
        {
            return new LuaTemplateType(prefixName, name);
        }

        return Builtin.Unknown;
    }
}
