using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Compile;

public static class TypeCompiler
{
    public static void Compile(LuaDocTypeSyntax typeSyntax, LuaCommentSyntax commentSyntax, TypeContext context)
    {
        var stack = new Stack<LuaType>();
        try
        {
            CompileType(typeSyntax, commentSyntax, context, stack);

            if (stack.Count == 0)
            {
                return;
            }

            var realType = stack.Peek();
            context.AddRealType(typeSyntax.UniqueId, realType);
        }
        catch (LuaTypeCompilationCancel e)
        {
            // ignore
        }
    }

    private static void CompileType(LuaDocTypeSyntax typeSyntax, LuaCommentSyntax commentSyntax, TypeContext context,
        Stack<LuaType> stack)
    {
        switch (typeSyntax)
        {
            case LuaDocNameTypeSyntax nameTypeSyntax:
            {
                CompileNameType(nameTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocObjectTypeSyntax luaDocObjectTypeSyntax:
            {
                CompileObjectType(luaDocObjectTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocParenTypeSyntax luaDocParenTypeSyntax:
            {
                CompileParenType(luaDocParenTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocStringTemplateTypeSyntax luaDocTemplateTypeSyntax:
            {
                CompileStringTemplateType(luaDocTemplateTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocGenericTypeSyntax genericTypeSyntax:
            {
                CompileGenericType(genericTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocIndexAccessTypeSyntax luaDocIndexAccessTypeSyntax:
            {
                CompileIndexAccessType(luaDocIndexAccessTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocIntersectionTypeSyntax luaDocIntersectionTypeSyntax:
            {
                CompileIntersectionType(luaDocIntersectionTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocInTypeSyntax luaDocInTypeSyntax:
            {
                CompileInType(luaDocInTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocKeyOfTypeSyntax luaDocKeyOfTypeSyntax:
            {
                CompileKeyOfType(luaDocKeyOfTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocLiteralTypeSyntax luaDocLiteralTypeSyntax:
            {
                CompileLiteralType(luaDocLiteralTypeSyntax, commentSyntax, context, stack);
                break;
            }
            // case LuaDocMappedKeysSyntax luaDocMappedKeysSyntax:
            // {
            //
            //     break;
            // }
            case LuaDocMappedTypeSyntax luaDocMappedTypeSyntax:
            {
                CompileMappedType(luaDocMappedTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocUnionTypeSyntax unionTypeSyntax:
            {
                CompileUnionType(unionTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocVariadicTypeSyntax luaDocVariadicTypeSyntax:
            {
                CompileVariadicType(luaDocVariadicTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocArrayTypeSyntax arrayTypeSyntax:
            {
                CompileArrayType(arrayTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocConditionalTypeSyntax luaDocConditionalTypeSyntax:
            {
                CompileConditionalType(luaDocConditionalTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocExpandTypeSyntax luaDocExpandTypeSyntax:
            {
                CompileExpandType(luaDocExpandTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocExtendTypeSyntax luaDocExtendTypeSyntax:
            {
                CompileExtendType(luaDocExtendTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocTupleTypeSyntax tupleTypeSyntax:
            {
                CompileTupleType(tupleTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocFuncTypeSyntax funcTypeSyntax:
            {
                CompileFuncType(funcTypeSyntax, commentSyntax, context, stack);
                break;
            }
        }
    }

    private static void CompileVariadicType(LuaDocVariadicTypeSyntax luaDocVariadicTypeSyntax,
        LuaCommentSyntax commentSyntax, TypeContext context, Stack<LuaType> stack)
    {
        if (luaDocVariadicTypeSyntax.Name?.RepresentText is { } name)
        {
            var type = context.FindType(name, commentSyntax);
            if (type is not null)
            {
                stack.Push(type);
            }
            else
            {
                context.AddDiagnostic(Diagnostic.Error(DiagnosticCode.TypeNotFound, "Type not found",
                    luaDocVariadicTypeSyntax.Range));
                throw new LuaTypeCompilationCancel();
            }
        }
    }

    private static void CompileExtendType(LuaDocExtendTypeSyntax luaDocExtendTypeSyntax, LuaCommentSyntax commentSyntax,
        TypeContext context, Stack<LuaType> stack)
    {
        if (luaDocExtendTypeSyntax is {BaseType: { } baseTypeSyntax, ExtendType: { } extendTypeSyntax})
        {
            CompileType(baseTypeSyntax, commentSyntax, context, stack);
            CompileType(extendTypeSyntax, commentSyntax, context, stack);
            var extendType = stack.Pop();
            var baseType = stack.Pop();
            stack.Push(new LuaExtendType(baseType, extendType));
        }
        else
        {
            context.AddDiagnostic(Diagnostic.Error(DiagnosticCode.SyntaxError, "UnComplete extend type",
                luaDocExtendTypeSyntax.ExtendToken.Range));
        }
    }

    private static void CompileExpandType(LuaDocExpandTypeSyntax luaDocExpandTypeSyntax, LuaCommentSyntax commentSyntax,
        TypeContext context, Stack<LuaType> stack)
    {
        if (luaDocExpandTypeSyntax.Name?.RepresentText is { } name)
        {
            var type = new LuaExpandTplType(name);
            stack.Push(type);
        }
    }

    private static void CompileConditionalType(LuaDocConditionalTypeSyntax luaDocConditionalTypeSyntax,
        LuaCommentSyntax commentSyntax, TypeContext context, Stack<LuaType> stack)
    {
        if (luaDocConditionalTypeSyntax is
            {
                CheckType: { } checkTypeSyntax, TrueType: { } trueTypeSyntax,
                FalseType: { } falseTypeSyntax
            })
        {
            CompileType(checkTypeSyntax, commentSyntax, context, stack);
            CompileType(trueTypeSyntax, commentSyntax, context, stack);
            CompileType(falseTypeSyntax, commentSyntax, context, stack);
            var falseType = stack.Pop();
            var trueType = stack.Pop();
            var checkType = stack.Pop();
            if (checkType is LuaBooleanLiteralType booleanLiteralType)
            {
                stack.Push(booleanLiteralType.Value ? trueType : falseType);
            }
            else
            {
                stack.Push(new LuaTernaryType(checkType, trueType, falseType));
            }
        }
        else
        {
            context.AddDiagnostic(Diagnostic.Error(DiagnosticCode.SyntaxError, "UnComplete conditional type",
                luaDocConditionalTypeSyntax.QuestionToken.Range));
        }
    }

    private static void CompileMappedType(LuaDocMappedTypeSyntax luaDocMappedTypeSyntax, LuaCommentSyntax commentSyntax,
        TypeContext context, Stack<LuaType> stack)
    {
        throw new NotImplementedException();
    }

    private static void CompileLiteralType(LuaDocLiteralTypeSyntax luaDocLiteralTypeSyntax,
        LuaCommentSyntax commentSyntax, TypeContext context, Stack<LuaType> stack)
    {
        switch (luaDocLiteralTypeSyntax)
        {
            case {Boolean.Kind: { } kind}:
                stack.Push(new LuaBooleanLiteralType(kind == LuaTokenKind.TkTrue));
                break;
            case {Integer: { } integer}:
                stack.Push(new LuaIntegerLiteralType(integer.Value));
                break;
            case {String: { } str}:
                stack.Push(new LuaStringLiteralType(str.Value));
                break;
        }
    }

    private static void CompileKeyOfType(LuaDocKeyOfTypeSyntax luaDocKeyOfTypeSyntax, LuaCommentSyntax commentSyntax,
        TypeContext context, Stack<LuaType> stack)
    {
        if (luaDocKeyOfTypeSyntax.Type is { } typeSyntax)
        {
            CompileType(typeSyntax, commentSyntax, context, stack);
            var type = stack.Pop();
            if (TryGetKeys(type, out var list, commentSyntax, context))
            {
                var stringLiteralTypes = list.Select(s => new LuaStringLiteralType(s));
                var luaUnionType = new LuaUnionType(stringLiteralTypes.Cast<LuaType>().ToList());
                stack.Push(luaUnionType);
            }
            else
            {
                stack.Push(new LuaKeyOfType(type));
            }
        }
    }

    // TODO
    private static bool TryGetKeys(LuaType type, out List<string> result, LuaCommentSyntax commentSyntax,
        TypeContext context)
    {
        throw new NotImplementedException();
    }

    private static void CompileInType(LuaDocInTypeSyntax luaDocInTypeSyntax, LuaCommentSyntax commentSyntax,
        TypeContext context, Stack<LuaType> stack)
    {
        if (luaDocInTypeSyntax is {KeyType: { } keyType, IndexType: { } indexType})
        {
            // if (keyType is )
        }
    }

    private static void CompileIntersectionType(LuaDocIntersectionTypeSyntax luaDocIntersectionTypeSyntax,
        LuaCommentSyntax commentSyntax, TypeContext context, Stack<LuaType> stack)
    {
        var count = 0;
        foreach (var luaDocTypeSyntax in luaDocIntersectionTypeSyntax.IntersectionTypes)
        {
            CompileType(luaDocTypeSyntax, commentSyntax, context, stack);
            count++;
        }

        if (count > 1)
        {
            var types = new List<LuaType>();
            for (var i = count - 1; i >= 0; i--)
            {
                var type = stack.Pop();
                types.Add(type);
            }
            // TODO calculate intersection

            stack.Push(new LuaIntersectionType(types));
        }
    }

    private static void CompileIndexAccessType(LuaDocIndexAccessTypeSyntax luaDocIndexAccessTypeSyntax,
        LuaCommentSyntax commentSyntax, TypeContext context, Stack<LuaType> stack)
    {
        if (luaDocIndexAccessTypeSyntax is {BaseType: { } baseTypeSyntax, IndexType: { } indexTypeSyntax})
        {
            CompileType(baseTypeSyntax, commentSyntax, context, stack);
            CompileType(indexTypeSyntax, commentSyntax, context, stack);
            var indexType = stack.Pop();
            var baseType = stack.Pop();
            stack.Push(new LuaIndexedAccessType(baseType, indexType));
        }
        else
        {
            context.AddDiagnostic(Diagnostic.Error(DiagnosticCode.SyntaxError, "UnComplete index access type",
                luaDocIndexAccessTypeSyntax.Range));
        }
    }

    private static void CompileStringTemplateType(LuaDocStringTemplateTypeSyntax luaDocStringTemplateTypeSyntax,
        LuaCommentSyntax commentSyntax, TypeContext context, Stack<LuaType> stack)
    {
        if (luaDocStringTemplateTypeSyntax is {TemplateName.Name: { } templateName})
        {
            var type = context.FindType(templateName, commentSyntax);
            if (type is LuaTplType)
            {
                string prefix = string.Empty;
                if (luaDocStringTemplateTypeSyntax.PrefixName is {RepresentText: {} prefixName})
                {
                    prefix = prefixName;
                }

                var stringTplType = new LuaStrTplType(prefix, templateName);
                stack.Push(stringTplType);
                return;
            }
        }

        context.AddDiagnostic(Diagnostic.Error(DiagnosticCode.SyntaxError, "UnComplete string template type",
            luaDocStringTemplateTypeSyntax.Range));
    }

    private static void CompileParenType(LuaDocParenTypeSyntax luaDocParenTypeSyntax, LuaCommentSyntax commentSyntax,
        TypeContext context, Stack<LuaType> stack)
    {
        if (luaDocParenTypeSyntax.Type is { } typeSyntax)
        {
            CompileType(typeSyntax, commentSyntax, context, stack);
        }
    }

    private static void CompileObjectType(LuaDocObjectTypeSyntax luaDocObjectTypeSyntax, LuaCommentSyntax commentSyntax,
        TypeContext context, Stack<LuaType> stack)
    {
        throw new NotImplementedException();
    }

    private static void CompileUnionType(LuaDocUnionTypeSyntax unionTypeSyntax, LuaCommentSyntax commentSyntax,
        TypeContext context, Stack<LuaType> stack)
    {
        var count = 0;
        foreach (var luaDocTypeSyntax in unionTypeSyntax.UnionTypes)
        {
            CompileType(luaDocTypeSyntax, commentSyntax, context, stack);
            count++;
        }

        if (count > 1)
        {
            var types = new List<LuaType>();
            for (var i = count - 1; i >= 0; i--)
            {
                var type = stack.Pop();
                if (type is LuaUnionType unionType)
                {
                    types.AddRange(unionType.TypeList);
                }
                else
                {
                    types.Add(type);
                }
            }

            stack.Push(new LuaUnionType(types));
        }
    }

    private static void CompileArrayType(LuaDocArrayTypeSyntax arrayTypeSyntax, LuaCommentSyntax commentSyntax,
        TypeContext context, Stack<LuaType> stack)
    {
        if (arrayTypeSyntax.BaseType is null)
        {
            throw new LuaTypeCompilationCancel();
        }

        CompileType(arrayTypeSyntax.BaseType, commentSyntax, context, stack);
        var baseType = stack.Pop();
        stack.Push(new LuaArrayType(baseType));
    }

    private static void CompileTupleType(LuaDocTupleTypeSyntax tupleTypeSyntax, LuaCommentSyntax commentSyntax,
        TypeContext context, Stack<LuaType> stack)
    {
        var count = 0;
        foreach (var luaDocTypeSyntax in tupleTypeSyntax.TypeList)
        {
            CompileType(luaDocTypeSyntax, commentSyntax, context, stack);
            count++;
        }

        if (count > 1)
        {
            var types = new List<LuaType>();
            for (var i = count - 1; i >= 0; i--)
            {
                var type = stack.Pop();
                types.Add(type);
            }

            stack.Push(new LuaTupleType(types));
        }
    }

    private static void CompileFuncType(LuaDocFuncTypeSyntax funcTypeSyntax, LuaCommentSyntax commentSyntax,
        TypeContext context, Stack<LuaType> stack)
    {
        throw new NotImplementedException();
    }

    private static void CompileGenericType(LuaDocGenericTypeSyntax genericTypeSyntax, LuaCommentSyntax commentSyntax,
        TypeContext context, Stack<LuaType> stack)
    {
        // var type = context.FindType(genericTypeSyntax.Name);
        // if (type is not null)
        // {
        //     stack.Push(type);
        // }
    }

    private static void CompileNameType(LuaDocNameTypeSyntax nameTypeSyntax, LuaCommentSyntax commentSyntax,
        TypeContext context, Stack<LuaType> stack)
    {
        if (nameTypeSyntax.Name?.RepresentText is { } name)
        {
            var type = context.FindType(name, commentSyntax);
            if (type is not null)
            {
                stack.Push(type);
            }
            else
            {
                context.AddDiagnostic(Diagnostic.Error(DiagnosticCode.TypeNotFound, "Type not found",
                    nameTypeSyntax.Range));
                throw new LuaTypeCompilationCancel();
            }
        }
    }
}
