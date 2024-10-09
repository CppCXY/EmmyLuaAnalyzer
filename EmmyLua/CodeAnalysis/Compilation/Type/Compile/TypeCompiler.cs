using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Compile;

public class TypeCompiler(LuaCommentSyntax commentSyntax, TypeContext context)
{
    private Stack<LuaType> Stack { get; } = new();

    public static void Compile(LuaDocTypeSyntax typeSyntax, LuaCommentSyntax commentSyntax, TypeContext context)
    {
        var compiler = new TypeCompiler(commentSyntax, context);
        try
        {
            compiler.CompileType(typeSyntax);

            if (compiler.Stack.Count == 0)
            {
                return;
            }

            var realType = compiler.Stack.Peek();
            context.AddRealType(typeSyntax.UniqueId, realType);
        }
        catch (LuaTypeCompilationCancel e)
        {
            context.AddDiagnostic(Diagnostic.Error(e.Code, e.Message, e.Range));
        }
    }

    private void CompileType(LuaDocTypeSyntax typeSyntax)
    {
        switch (typeSyntax)
        {
            case LuaDocNameTypeSyntax nameTypeSyntax:
            {
                CompileNameType(nameTypeSyntax);
                break;
            }
            case LuaDocObjectTypeSyntax luaDocObjectTypeSyntax:
            {
                CompileObjectType(luaDocObjectTypeSyntax);
                break;
            }
            case LuaDocParenTypeSyntax luaDocParenTypeSyntax:
            {
                CompileParenType(luaDocParenTypeSyntax);
                break;
            }
            case LuaDocStringTemplateTypeSyntax luaDocTemplateTypeSyntax:
            {
                CompileStringTemplateType(luaDocTemplateTypeSyntax);
                break;
            }
            case LuaDocGenericTypeSyntax genericTypeSyntax:
            {
                CompileGenericType(genericTypeSyntax);
                break;
            }
            case LuaDocIndexAccessTypeSyntax luaDocIndexAccessTypeSyntax:
            {
                CompileIndexAccessType(luaDocIndexAccessTypeSyntax);
                break;
            }
            case LuaDocIntersectionTypeSyntax luaDocIntersectionTypeSyntax:
            {
                CompileIntersectionType(luaDocIntersectionTypeSyntax);
                break;
            }
            case LuaDocInTypeSyntax luaDocInTypeSyntax:
            {
                CompileInType(luaDocInTypeSyntax);
                break;
            }
            case LuaDocKeyOfTypeSyntax luaDocKeyOfTypeSyntax:
            {
                CompileKeyOfType(luaDocKeyOfTypeSyntax);
                break;
            }
            case LuaDocLiteralTypeSyntax luaDocLiteralTypeSyntax:
            {
                CompileLiteralType(luaDocLiteralTypeSyntax);
                break;
            }
            // case LuaDocMappedKeysSyntax luaDocMappedKeysSyntax:
            // {
            //
            //     break;
            // }
            case LuaDocMappedTypeSyntax luaDocMappedTypeSyntax:
            {
                CompileMappedType(luaDocMappedTypeSyntax);
                break;
            }
            case LuaDocUnionTypeSyntax unionTypeSyntax:
            {
                CompileUnionType(unionTypeSyntax);
                break;
            }
            case LuaDocVariadicTypeSyntax luaDocVariadicTypeSyntax:
            {
                CompileVariadicType(luaDocVariadicTypeSyntax);
                break;
            }
            case LuaDocArrayTypeSyntax arrayTypeSyntax:
            {
                CompileArrayType(arrayTypeSyntax);
                break;
            }
            case LuaDocConditionalTypeSyntax luaDocConditionalTypeSyntax:
            {
                CompileConditionalType(luaDocConditionalTypeSyntax);
                break;
            }
            case LuaDocExpandTypeSyntax luaDocExpandTypeSyntax:
            {
                CompileExpandType(luaDocExpandTypeSyntax);
                break;
            }
            case LuaDocExtendTypeSyntax luaDocExtendTypeSyntax:
            {
                CompileExtendType(luaDocExtendTypeSyntax);
                break;
            }
            case LuaDocTupleTypeSyntax tupleTypeSyntax:
            {
                CompileTupleType(tupleTypeSyntax);
                break;
            }
            case LuaDocFuncTypeSyntax funcTypeSyntax:
            {
                CompileFuncType(funcTypeSyntax);
                break;
            }
        }
    }

    private void CompileVariadicType(LuaDocVariadicTypeSyntax luaDocVariadicTypeSyntax)
    {
        if (luaDocVariadicTypeSyntax.Name?.RepresentText is { } name)
        {
            var type = context.FindType(name, commentSyntax);
            if (type is null)
            {
                throw new LuaTypeCompilationCancel(DiagnosticCode.TypeNotFound, "Type not found",
                    luaDocVariadicTypeSyntax.Range);
            }

            Stack.Push(type);
            return;
        }

        throw new LuaTypeCompilationCancel(DiagnosticCode.SyntaxError, "UnComplete variadic type",
            luaDocVariadicTypeSyntax.Range);
    }

    private void CompileExtendType(LuaDocExtendTypeSyntax luaDocExtendTypeSyntax)
    {
        if (luaDocExtendTypeSyntax is { BaseType: { } baseTypeSyntax, ExtendType: { } extendTypeSyntax })
        {
            CompileType(baseTypeSyntax);
            CompileType(extendTypeSyntax);
            var extendType = Stack.Pop();
            var baseType = Stack.Pop();
            // TODO check extend type
            // if (baseType.IsSubTypeOf(extendType))
            // {
            //
            // }

            Stack.Push(new LuaExtendType(baseType, extendType));
            return;
        }

        throw new LuaTypeCompilationCancel(DiagnosticCode.SyntaxError, "UnComplete extend type",
            luaDocExtendTypeSyntax.ExtendToken.Range);
    }

    // T...
    private void CompileExpandType(LuaDocExpandTypeSyntax luaDocExpandTypeSyntax)
    {
        if (luaDocExpandTypeSyntax.Name?.RepresentText is { } name)
        {
            var type = context.FindType(name, commentSyntax);
            if (type is not LuaTplType)
            {
                throw new LuaTypeCompilationCancel(DiagnosticCode.TypeNotFound, "Type is not a template type",
                    luaDocExpandTypeSyntax.Range);
            }

            Stack.Push(new LuaExpandTplType(name));
            return;
        }

        throw new LuaTypeCompilationCancel(DiagnosticCode.SyntaxError, "UnComplete expand type",
            luaDocExpandTypeSyntax.Range);
    }

    private void CompileConditionalType(LuaDocConditionalTypeSyntax luaDocConditionalTypeSyntax)
    {
        if (luaDocConditionalTypeSyntax is
            {
                CheckType: { } checkTypeSyntax,
                TrueType: { } trueTypeSyntax,
                FalseType: { } falseTypeSyntax
            })
        {
            CompileType(checkTypeSyntax);
            CompileType(trueTypeSyntax);
            CompileType(falseTypeSyntax);
            var falseType = Stack.Pop();
            var trueType = Stack.Pop();
            var checkType = Stack.Pop();
            if (checkType is LuaBooleanLiteralType booleanLiteralType)
            {
                Stack.Push(booleanLiteralType.Value ? trueType : falseType);
            }
            else
            {
                Stack.Push(new LuaTernaryType(checkType, trueType, falseType));
            }

            return;
        }

        throw new LuaTypeCompilationCancel(DiagnosticCode.SyntaxError, "UnComplete conditional type",
            luaDocConditionalTypeSyntax.QuestionToken.Range);
    }

    private void CompileMappedType(LuaDocMappedTypeSyntax luaDocMappedTypeSyntax)
    {
        throw new NotImplementedException();
    }

    private void CompileLiteralType(LuaDocLiteralTypeSyntax luaDocLiteralTypeSyntax)
    {
        switch (luaDocLiteralTypeSyntax)
        {
            case { Boolean.Kind: { } kind }:
                Stack.Push(new LuaBooleanLiteralType(kind == LuaTokenKind.TkTrue));
                return;
            case { Integer: { } integer }:
                Stack.Push(new LuaIntegerLiteralType(integer.Value));
                return;
            case { String: { } str }:
                Stack.Push(new LuaStringLiteralType(str.Value));
                return;
        }

        throw new LuaTypeCompilationCancel(DiagnosticCode.SyntaxError, "UnComplete literal type",
            luaDocLiteralTypeSyntax.Range);
    }

    private void CompileKeyOfType(LuaDocKeyOfTypeSyntax luaDocKeyOfTypeSyntax)
    {
        if (luaDocKeyOfTypeSyntax.Type is { } typeSyntax)
        {
            CompileType(typeSyntax);
            var type = Stack.Pop();
            if (TryGetKeys(type, out var list))
            {
                var stringLiteralTypes = list.Select(s => new LuaStringLiteralType(s));
                var luaUnionType = new LuaUnionType(stringLiteralTypes.Cast<LuaType>().ToList());
                Stack.Push(luaUnionType);
            }
            else
            {
                Stack.Push(new LuaKeyOfType(type));
            }

            return;
        }

        throw new LuaTypeCompilationCancel(DiagnosticCode.SyntaxError, "UnComplete key of type",
            luaDocKeyOfTypeSyntax.KeyOfToken.Range);
    }

    private bool TryGetKeys(LuaType type, out List<string> result)
    {
        result = [];
        if (type is LuaNamedType namedType)
        {
            var typeInfo = context.Compilation.TypeManager.FindTypeInfo(namedType);
            if (typeInfo is null)
            {
                return false;
            }

            if (typeInfo.Declarations?.Keys.ToList() is { } keys)
            {
                result = keys;
            }
        }

        return false;
    }

    private void CompileInType(LuaDocInTypeSyntax luaDocInTypeSyntax)
    {
        if (luaDocInTypeSyntax is { KeyType.Name.RepresentText: { } name, IndexType: { } indexType })
        {
            CompileType(indexType);
            var baseType = Stack.Pop();
            var inType = new LuaInType(name, baseType);
            Stack.Push(inType);
            return;
        }

        throw new LuaTypeCompilationCancel(DiagnosticCode.SyntaxError, "UnComplete in type",
            luaDocInTypeSyntax.InToken.Range);
    }

    private void CompileIntersectionType(LuaDocIntersectionTypeSyntax luaDocIntersectionTypeSyntax)
    {
        var count = 0;
        foreach (var luaDocTypeSyntax in luaDocIntersectionTypeSyntax.IntersectionTypes)
        {
            CompileType(luaDocTypeSyntax);
            count++;
        }

        if (count == 0)
        {
            throw new LuaTypeCompilationCancel(DiagnosticCode.SyntaxError, "UnComplete intersection type",
                luaDocIntersectionTypeSyntax.Range);
        }

        var types = new List<LuaType>();
        for (var i = count - 1; i >= 0; i--)
        {
            var type = Stack.Pop();
            types.Add(type);
        }
        // TODO calculate intersection

        Stack.Push(new LuaIntersectionType(types));
    }

    private void CompileIndexAccessType(LuaDocIndexAccessTypeSyntax luaDocIndexAccessTypeSyntax)
    {
        if (luaDocIndexAccessTypeSyntax is { BaseType: { } baseTypeSyntax, IndexType: { } indexTypeSyntax })
        {
            CompileType(baseTypeSyntax);
            CompileType(indexTypeSyntax);
            var indexType = Stack.Pop();
            var baseType = Stack.Pop();

            Stack.Push(new LuaIndexedAccessType(baseType, indexType));
            return;
        }

        throw new LuaTypeCompilationCancel(DiagnosticCode.SyntaxError, "UnComplete index access type",
            luaDocIndexAccessTypeSyntax.Range);
    }

    private void CompileStringTemplateType(LuaDocStringTemplateTypeSyntax luaDocStringTemplateTypeSyntax)
    {
        if (luaDocStringTemplateTypeSyntax is { TemplateName.Name: { } templateName })
        {
            var type = context.FindType(templateName, commentSyntax);
            if (type is not LuaTplType)
            {
                throw new LuaTypeCompilationCancel(DiagnosticCode.TypeNotFound, "Type is not a template type",
                    luaDocStringTemplateTypeSyntax.Range);
            }

            var prefix = string.Empty;
            if (luaDocStringTemplateTypeSyntax.PrefixName is { RepresentText: { } prefixName })
            {
                prefix = prefixName;
            }

            var stringTplType = new LuaStrTplType(prefix, templateName);
            Stack.Push(stringTplType);
            return;
        }

        throw new LuaTypeCompilationCancel(DiagnosticCode.SyntaxError, "UnComplete string template type",
            luaDocStringTemplateTypeSyntax.Range);
    }

    private void CompileParenType(LuaDocParenTypeSyntax luaDocParenTypeSyntax)
    {
        if (luaDocParenTypeSyntax.Type is { } typeSyntax)
        {
            CompileType(typeSyntax);
            return;
        }

        throw new LuaTypeCompilationCancel(DiagnosticCode.SyntaxError, "UnComplete paren type",
            luaDocParenTypeSyntax.Range);
    }

    private static void CompileObjectType(LuaDocObjectTypeSyntax luaDocObjectTypeSyntax)
    {
        throw new NotImplementedException();
    }

    private void CompileUnionType(LuaDocUnionTypeSyntax unionTypeSyntax)
    {
        var count = 0;
        foreach (var luaDocTypeSyntax in unionTypeSyntax.UnionTypes)
        {
            CompileType(luaDocTypeSyntax);
            count++;
        }

        if (count == 0)
        {
            throw new LuaTypeCompilationCancel(DiagnosticCode.SyntaxError, "UnComplete union type",
                unionTypeSyntax.Range);
        }

        var types = new List<LuaType>();
        for (var i = count - 1; i >= 0; i--)
        {
            var type = Stack.Pop();
            if (type is LuaUnionType unionType)
            {
                types.AddRange(unionType.TypeList);
            }
            else
            {
                types.Add(type);
            }
        }

        Stack.Push(new LuaUnionType(types));
    }

    private void CompileArrayType(LuaDocArrayTypeSyntax arrayTypeSyntax)
    {
        if (arrayTypeSyntax.BaseType is null)
        {
            throw new LuaTypeCompilationCancel(DiagnosticCode.SyntaxError, "UnComplete array type",
                arrayTypeSyntax.Range);
        }

        CompileType(arrayTypeSyntax.BaseType);
        var baseType = Stack.Pop();
        Stack.Push(new LuaArrayType(baseType));
    }

    private void CompileTupleType(LuaDocTupleTypeSyntax tupleTypeSyntax)
    {
        var count = 0;
        foreach (var luaDocTypeSyntax in tupleTypeSyntax.TypeList)
        {
            CompileType(luaDocTypeSyntax);
            count++;
        }

        if (count == 1)
        {
            throw new LuaTypeCompilationCancel(DiagnosticCode.SyntaxError, "Tuple type must have more than one type",
                tupleTypeSyntax.Range);
        }

        var types = new List<LuaType>();
        for (var i = count - 1; i >= 0; i--)
        {
            var type = Stack.Pop();
            types.Add(type);
        }

        Stack.Push(new LuaTupleType(types));
    }

    private static void CompileFuncType(LuaDocFuncTypeSyntax funcTypeSyntax)
    {
        throw new NotImplementedException();
    }

    private static void CompileGenericType(LuaDocGenericTypeSyntax genericTypeSyntax)
    {
        // var type = context.FindType(genericTypeSyntax.Name);
        // if (type is not null)
        // {
        //     Stack.Push(type);
        // }
    }

    private void CompileNameType(LuaDocNameTypeSyntax nameTypeSyntax)
    {
        if (nameTypeSyntax.Name?.RepresentText is { } name)
        {
            var type = context.FindType(name, commentSyntax);
            if (type is null)
            {
                throw new LuaTypeCompilationCancel(DiagnosticCode.TypeNotFound, "Type not found",
                    nameTypeSyntax.Range);
            }

            Stack.Push(type);
            return;
        }

        throw new LuaTypeCompilationCancel(DiagnosticCode.SyntaxError, "UnComplete name type",
            nameTypeSyntax.Range);
    }
}
