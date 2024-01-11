using System.Diagnostics;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;

public static class TypeInfer
{
    public static ILuaType InferType(LuaDocTypeSyntax type, SearchContext context)
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

    private static ILuaType InferTableType(LuaDocTableTypeSyntax tableType, SearchContext context)
    {
        var className = context.GetUniqueId(tableType);
        var ty = context.FindLuaType(className);
        return ty;
    }

    private static ILuaType InferArrayType(LuaDocArrayTypeSyntax arrayType, SearchContext context)
    {
        var baseTy = context.Infer(arrayType.BaseType);
        return new LuaArray(baseTy);
    }

    private static ILuaType InferUnionType(LuaDocUnionTypeSyntax unionType, SearchContext context)
    {
        var unionTy = new LuaUnion();
        var types = unionType.UnionTypes.Select(context.Infer);
        foreach (var type in types)
        {
            unionTy.UnionType(type);
        }

        return unionTy;
    }

    private static ILuaType InferLiteralType(LuaDocLiteralTypeSyntax literalType, SearchContext context)
    {
        if (literalType.Integer != null)
        {
            return new LuaLiteral(literalType.Integer);
        }
        else if (literalType.String != null)
        {
            return new LuaLiteral(literalType.String);
        }
        else
        {
            return context.Compilation.Builtin.Unknown;
        }
    }

    public static ILuaType InferFuncType(LuaDocFuncTypeSyntax funcType, SearchContext context)
    {
        var method = context.Compilation.StubIndexImpl.Methods.Get(funcType).FirstOrDefault();
        if (method is not null)
        {
            return method;
        }

        return context.Compilation.Builtin.Unknown;
    }

    private static ILuaType InferNameType(LuaDocNameTypeSyntax nameType, SearchContext context)
    {
        if (nameType.Name != null)
        {
            var name = nameType.Name.RepresentText;
            var ty = context.FindLuaType(name);
            return ty;
        }
        else
        {
            return context.Compilation.Builtin.Unknown;
        }
    }

    private static ILuaType InferParenType(LuaDocParenTypeSyntax parenType, SearchContext context)
    {
        return parenType.Type != null
            ? InferType(parenType.Type, context)
            : context.Compilation.Builtin.Unknown;
    }

    private static ILuaType InferTupleType(LuaDocTupleTypeSyntax tupleType, SearchContext context)
    {
        var types = tupleType.TypeList.Select(context.Infer).ToList();
        return new LuaTuple(types);
    }

    private static ILuaType InferGenericType(LuaDocGenericTypeSyntax genericType, SearchContext context)
    {
        var baseType = context.Infer(genericType.Name);
        if (baseType is IGenericBase genericBase)
        {
            var typeArgs = genericType.GenericArgs.Select(context.Infer).ToList();
            return GenericInfer.InferGeneric(genericBase, typeArgs, context);
        }

        return context.Compilation.Builtin.Unknown;
    }
}
