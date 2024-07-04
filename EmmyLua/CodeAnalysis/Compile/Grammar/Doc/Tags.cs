using EmmyLua.CodeAnalysis.Compile.Lexer;
using EmmyLua.CodeAnalysis.Compile.Parser;
using EmmyLua.CodeAnalysis.Syntax.Kind;

namespace EmmyLua.CodeAnalysis.Compile.Grammar.Doc;

public static class TagParser
{
    public static CompleteMarker Tag(LuaDocParser p)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (p.Current)
        {
            case LuaTokenKind.TkTagClass:
            {
                return TagClass(p);
            }
            case LuaTokenKind.TkTagEnum:
            {
                return TagEnum(p);
            }
            case LuaTokenKind.TkTagInterface:
            {
                return TagInterface(p);
            }
            case LuaTokenKind.TkTagAlias:
            {
                return TagAlias(p);
            }
            case LuaTokenKind.TkTagField:
            {
                return TagField(p);
            }
            case LuaTokenKind.TkTagType:
            {
                return TagType(p);
            }
            case LuaTokenKind.TkTagAs:
            {
                return TagAs(p);
            }
            case LuaTokenKind.TkTagParam:
            {
                return TagParam(p);
            }
            case LuaTokenKind.TkTagReturn:
            {
                return TagReturn(p);
            }
            case LuaTokenKind.TkTagGeneric:
            {
                return TagGeneric(p);
            }
            case LuaTokenKind.TkTagSee:
            {
                return TagSee(p);
            }
            case LuaTokenKind.TkTagOverload:
            {
                return TagOverload(p);
            }
            case LuaTokenKind.TkTagAsync:
            {
                return SimpleTag(p, LuaSyntaxKind.DocAsync);
            }
            case LuaTokenKind.TkTagCast:
            {
                return TagCast(p);
            }
            case LuaTokenKind.TkTagDeprecated:
            {
                return SimpleTag(p, LuaSyntaxKind.DocDeprecated);
            }
            case LuaTokenKind.TkTagVisibility:
            {
                return SimpleTag(p, LuaSyntaxKind.DocVisibility);
            }
            case LuaTokenKind.TkTagDiagnostic:
            {
                return TagDiagnostic(p);
            }
            case LuaTokenKind.TkTagVersion:
            {
                return TagVersion(p);
            }
            case LuaTokenKind.TkTagNodiscard:
            {
                return SimpleTag(p, LuaSyntaxKind.DocNodiscard);
            }
            case LuaTokenKind.TkTagOperator:
            {
                return TagOperator(p);
            }
            case LuaTokenKind.TkTagModule:
            {
                return TagModule(p);
            }
            case LuaTokenKind.TkTagMapping:
            {
                return TagMapping(p);
            }
            case LuaTokenKind.TkTagMeta:
            {
                return SimpleTag(p, LuaSyntaxKind.DocMeta);
            }
            case LuaTokenKind.TkTagOther:
            {
                return SimpleTag(p, LuaSyntaxKind.DocOther);
            }
            case LuaTokenKind.TkWhitespace:
            {
                p.Bump();
                p.Events.Add(new MarkEvent.Error("expected <tag> but got whitespace"));
                p.SetState(LuaDocLexerState.Trivia);
                p.Accept(LuaTokenKind.TkDocTrivia);
                return CompleteMarker.Empty;
            }
            default:
            {
                p.Events.Add(new MarkEvent.Error($"expected <tag> but got {p.Current}"));
                p.SetState(LuaDocLexerState.Trivia);
                p.Accept(LuaTokenKind.TkDocTrivia);
                return CompleteMarker.Empty;
            }
        }
    }

    public static CompleteMarker LongDocTag(LuaDocParser p)
    {
        return Tag(p);
    }

    private static CompleteMarker TagClass(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            if (p.Current is LuaTokenKind.TkLeftParen)
            {
                Attribute(p);
            }

            p.Expect(LuaTokenKind.TkName);
            var state = ClassSuffixState.Start;
            while (state is not ClassSuffixState.Finish)
            {
                ClassSuffix(p, ref state);
            }

            return m.Complete(p, LuaSyntaxKind.DocClass);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocClass, e.Message);
        }
    }

    enum ClassSuffixState
    {
        Start,
        Generic,
        Extend,
        Body,
        Finish
    }

    private static void ClassSuffix(LuaDocParser p, ref ClassSuffixState state)
    {
        switch (state)
        {
            case ClassSuffixState.Start:
            {
                if (p.Current is LuaTokenKind.TkLt)
                {
                    GenericDeclareList(p);
                    state = ClassSuffixState.Generic;
                }
                else if (p.Current is LuaTokenKind.TkColon)
                {
                    ExtensionTypeList(p);
                    state = ClassSuffixState.Extend;
                }
                else if (p.Current is LuaTokenKind.TkLeftBrace)
                {
                    Fields.DefineBody(p);
                    state = ClassSuffixState.Body;
                }
                else
                {
                    goto default;
                }

                break;
            }
            case ClassSuffixState.Generic:
            {
                if (p.Current is LuaTokenKind.TkColon)
                {
                    ExtensionTypeList(p);
                    state = ClassSuffixState.Extend;
                }
                else if (p.Current is LuaTokenKind.TkLeftBrace)
                {
                    Fields.DefineBody(p);
                    state = ClassSuffixState.Body;
                }
                else
                {
                    goto default;
                }

                break;
            }
            case ClassSuffixState.Extend:
            {
                if (p.Current is LuaTokenKind.TkLeftBrace)
                {
                    Fields.DefineBody(p);
                    state = ClassSuffixState.Body;
                }
                else
                {
                    goto default;
                }

                break;
            }
            default:
            {
                DescriptionParser.Description(p);
                state = ClassSuffixState.Finish;
                break;
            }
        }
    }

    private static CompleteMarker GenericParam(LuaDocParser p)
    {
        var m = p.Marker();
        try
        {
            p.Expect(LuaTokenKind.TkName);
            if (p.Current is LuaTokenKind.TkColon)
            {
                p.Bump();
                TypesParser.Type(p);
            }

            return m.Complete(p, LuaSyntaxKind.GenericParameter);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.GenericParameter, e.Message);
        }
    }

    private static CompleteMarker GenericDeclareList(LuaDocParser p)
    {
        var m = p.Marker();
        p.Bump();
        try
        {
            var cm = GenericParam(p);
            while (cm.IsComplete && p.Current is LuaTokenKind.TkComma)
            {
                p.Bump();
                cm = GenericParam(p);
            }

            p.Accept(LuaTokenKind.TkDots);
            p.Expect(LuaTokenKind.TkGt);
            return m.Complete(p, LuaSyntaxKind.GenericDeclareList);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.GenericDeclareList, e.Message);
        }
    }

    private static void EnumFields(LuaDocParser p)
    {
        if (p.Current is not LuaTokenKind.TkDocOr)
        {
            return;
        }

        var m = p.Marker();
        p.Bump();

        var cm2 = EnumField(p);
        while (cm2.IsComplete && p.Current is LuaTokenKind.TkDocOr)
        {
            p.Bump();
            cm2 = EnumField(p);
        }
    }

    public static CompleteMarker EnumField(LuaDocParser p)
    {
        var m = p.Marker();
        try
        {
            p.Expect(LuaTokenKind.TkName);
            DescriptionParser.InlineDescription(p);
            return m.Complete(p, LuaSyntaxKind.DocEnumField);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocEnumField, e.Message);
        }
    }

    /// <summary>
    /// ---@enum A @aaaaa
    /// ---| aaa @bbbb
    /// ---| cccc @dddd
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    private static CompleteMarker TagEnum(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            if (p.Current is LuaTokenKind.TkLeftParen)
            {
                Attribute(p);
            }

            p.Expect(LuaTokenKind.TkName);
            if (p.Current is LuaTokenKind.TkColon)
            {
                p.Bump();
                TypesParser.Type(p);
            }

            DescriptionParser.InlineDescription(p);

            EnumFields(p);
            DescriptionParser.Description(p);
            return m.Complete(p, LuaSyntaxKind.DocEnum);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocEnum, e.Message);
        }
    }

    private static CompleteMarker TagInterface(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            if (p.Current is LuaTokenKind.TkLeftParen)
            {
                Attribute(p);
            }

            p.Expect(LuaTokenKind.TkName);
            var state = ClassSuffixState.Start;
            while (state is not ClassSuffixState.Finish)
            {
                ClassSuffix(p, ref state);
            }

            return m.Complete(p, LuaSyntaxKind.DocInterface);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocInterface, e.Message);
        }
    }

    private static CompleteMarker TagAlias(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            if (p.Current is LuaTokenKind.TkLeftParen)
            {
                Attribute(p);
            }

            p.Expect(LuaTokenKind.TkName);
            TypesParser.AliasType(p);
            DescriptionParser.Description(p);
            return m.Complete(p, LuaSyntaxKind.DocAlias);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocAlias, e.Message);
        }
    }

    private static CompleteMarker TagField(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            if (p.Current is LuaTokenKind.TkLeftParen)
            {
                Attribute(p);
            }

            Fields.Field(p, false);
            return m.Complete(p, LuaSyntaxKind.DocField);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocField, e.Message);
        }
    }

    private static CompleteMarker TagType(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            TypesParser.TypeList(p);
            DescriptionParser.Description(p);
            return m.Complete(p, LuaSyntaxKind.DocType);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocType, e.Message);
        }
    }

    private static CompleteMarker TagParam(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            if (p.Current is LuaTokenKind.TkName or LuaTokenKind.TkDots)
            {
                p.Bump();
            }
            else
            {
                throw new UnexpectedTokenException("expected <name> or ...", p.Current);
            }

            p.Accept(LuaTokenKind.TkNullable);
            TypesParser.Type(p);
            DescriptionParser.Description(p);
            return m.Complete(p, LuaSyntaxKind.DocParam);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocParam, e.Message);
        }
    }

    private static CompleteMarker TagReturn(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            TypesParser.TypeList(p);
            DescriptionParser.Description(p);
            return m.Complete(p, LuaSyntaxKind.DocReturn);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocReturn, e.Message);
        }
    }

    private static CompleteMarker TagGeneric(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            var cm = GenericParam(p);
            while (cm.IsComplete && p.Current is LuaTokenKind.TkComma)
            {
                p.Bump();
                cm = GenericParam(p);
            }

            p.Accept(LuaTokenKind.TkDots);
            DescriptionParser.Description(p);
            return m.Complete(p, LuaSyntaxKind.DocGeneric);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocGeneric, e.Message);
        }
    }

    private static CompleteMarker TagSee(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.See);
        var m = p.Marker();
        p.Bump();
        try
        {
            p.Expect(LuaTokenKind.TkName);
            while (p.Current is LuaTokenKind.TkLen)
            {
                p.Bump();
                p.Expect(LuaTokenKind.TkName);
            }

            return m.Complete(p, LuaSyntaxKind.DocSee);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocSee, e.Message);
        }
    }

    private static CompleteMarker TagOverload(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            TypesParser.FunType(p);

            DescriptionParser.Description(p);
            return m.Complete(p, LuaSyntaxKind.DocOverload);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocOverload, e.Message);
        }
    }

    private static CompleteMarker TagCast(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            p.Expect(LuaTokenKind.TkName);

            TypesParser.Type(p);
            DescriptionParser.Description(p);
            return m.Complete(p, LuaSyntaxKind.DocCast);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocCast, e.Message);
        }
    }

    private static CompleteMarker SimpleTag(LuaDocParser p, LuaSyntaxKind kind)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        DescriptionParser.Description(p);
        return m.Complete(p, kind);
    }

    private static CompleteMarker TagDiagnostic(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            p.Expect(LuaTokenKind.TkName);
            // ReSharper disable once InvertIf
            if (p.Current is LuaTokenKind.TkColon)
            {
                p.Bump();
                DiagnosticList(p);
            }

            return m.Complete(p, LuaSyntaxKind.DocDiagnostic);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocDiagnostic, e.Message);
        }
    }

    private static CompleteMarker DiagnosticList(LuaDocParser p)
    {
        var m = p.Marker();
        try
        {
            p.Expect(LuaTokenKind.TkName);
            while (p.Current is LuaTokenKind.TkComma)
            {
                p.Bump();
                p.Expect(LuaTokenKind.TkName);
            }

            return m.Complete(p, LuaSyntaxKind.DiagnosticNameList);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DiagnosticNameList, e.Message);
        }
    }

    private static CompleteMarker TagVersion(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Version);
        var m = p.Marker();
        p.Bump();
        try
        {
            var cm = Version(p);
            while (cm.IsComplete && p.Current is LuaTokenKind.TkComma)
            {
                p.Bump();
                cm = Version(p);
            }

            DescriptionParser.Description(p);
            return m.Complete(p, LuaSyntaxKind.DocVersion);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocVersion, e.Message);
        }
    }

    private static CompleteMarker Version(LuaDocParser p)
    {
        var m = p.Marker();
        try
        {
            if (p.Current is LuaTokenKind.TkGt or LuaTokenKind.TkLt or LuaTokenKind.TkGe or LuaTokenKind.TkLe)
            {
                p.Bump();
            }

            if (p.Current is LuaTokenKind.TkName)
            {
                p.Bump();
            }

            p.Expect(LuaTokenKind.TkVersionNumber);
            return m.Complete(p, LuaSyntaxKind.Version);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.Version, e.Message);
        }
    }

    private static CompleteMarker TagAs(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        TypesParser.Type(p);
        DescriptionParser.Description(p);
        return m.Complete(p, LuaSyntaxKind.DocAs);
    }

    private static CompleteMarker TagOperator(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            p.Expect(LuaTokenKind.TkName);
            if (p.Current is LuaTokenKind.TkLeftParen)
            {
                p.Bump();
                var cm = TypesParser.Type(p);
                while (cm.IsComplete && p.Current is LuaTokenKind.TkComma)
                {
                    p.Bump();
                    cm = TypesParser.Type(p);
                }

                p.Expect(LuaTokenKind.TkRightParen);
            }

            p.Expect(LuaTokenKind.TkColon);
            TypesParser.Type(p);
            DescriptionParser.Description(p);
            return m.Complete(p, LuaSyntaxKind.DocOperator);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocOperator, e.Message);
        }
    }

    private static CompleteMarker TagModule(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            if (p.Current is LuaTokenKind.TkString)
            {
                p.Bump();
            }
            else if (p.Current is LuaTokenKind.TkName)
            {
                p.Bump();
            }
            else
            {
                throw new UnexpectedTokenException("expected <name> or <string>", p.Current);
            }

            DescriptionParser.Description(p);
            return m.Complete(p, LuaSyntaxKind.DocModule);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocModule, e.Message);
        }
    }

    private static CompleteMarker TagMapping(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            p.Expect(LuaTokenKind.TkName);
            DescriptionParser.Description(p);
            return m.Complete(p, LuaSyntaxKind.DocMapping);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocMapping, e.Message);
        }
    }

    private static void ExtensionTypeList(LuaDocParser p)
    {
        p.Bump();
        var cm = TypesParser.Type(p);
        while (cm.IsComplete && p.Current is LuaTokenKind.TkComma)
        {
            p.Bump();
            cm = TypesParser.Type(p);
        }
    }

    private static CompleteMarker Attribute(LuaDocParser p)
    {
        var m = p.Marker();
        try
        {
            p.Bump();
            p.Expect(LuaTokenKind.TkName);
            while (p.Current is LuaTokenKind.TkComma)
            {
                p.Bump();
                p.Expect(LuaTokenKind.TkName);
            }

            p.Expect(LuaTokenKind.TkRightParen);

            return m.Complete(p, LuaSyntaxKind.DocAttribute);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocAttribute, e.Message);
        }
    }
}
