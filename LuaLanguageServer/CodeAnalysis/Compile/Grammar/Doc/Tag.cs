using System.Diagnostics;
using LuaLanguageServer.CodeAnalysis.Compile.Lexer;
using LuaLanguageServer.CodeAnalysis.Compile.Parser;
using LuaLanguageServer.CodeAnalysis.Kind;

namespace LuaLanguageServer.CodeAnalysis.Compile.Grammar.Doc;

public static class TagParser
{
    public static CompleteMarker Tag(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Tag);
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
                throw new UnreachableException();
            }
        }
    }

    public static CompleteMarker LongDocTag(LuaDocParser p)
    {
        return Tag(p);
    }

    public static CompleteMarker EnumField(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        try
        {
            p.Expect(LuaTokenKind.TkString);
            AcceptDescription(p);
            return m.Complete(p, LuaSyntaxKind.DocEnumField);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocEnumField, e.Message);
        }
    }

    private static CompleteMarker TagClass(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
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
        ExpectDescription,
        Finish
    }

    private static void ClassSuffix(LuaDocParser p, ref ClassSuffixState state)
    {
        var rollbackPoint = p.GetRollbackPoint();

        switch (p.Current)
        {
            // generic
            case LuaTokenKind.TkLt:
            {
                if (state is not ClassSuffixState.Start)
                {
                    goto default;
                }

                GenericDeclareList(p);
                state = ClassSuffixState.Generic;
                break;
            }
            // extends
            case LuaTokenKind.TkColon:
            {
                if (state >= ClassSuffixState.ExpectDescription)
                {
                    goto default;
                }

                p.Bump();
                TypesParser.TypeList(p);
                state = ClassSuffixState.ExpectDescription;
                break;
            }
            // class table define
            case LuaTokenKind.TkLeftBrace:
            {
                if (state >= ClassSuffixState.ExpectDescription)
                {
                    goto default;
                }

                TypesParser.TableType(p);
                state = ClassSuffixState.ExpectDescription;
                break;
            }
            case LuaTokenKind.TkDocDescription:
            {
                p.Bump();
                state = ClassSuffixState.Finish;
                break;
            }
            default:
            {
                p.Rollback(rollbackPoint);
                p.SetState(LuaDocLexerState.Description);
                p.Accept(LuaTokenKind.TkDocDescription);
                state = ClassSuffixState.Finish;
                break;
            }
        }
    }

    private static CompleteMarker GenericDeclareList(LuaDocParser p)
    {
        var m = p.Marker();
        p.Bump();
        try
        {
            p.Expect(LuaTokenKind.TkName);
            while (p.Current is LuaTokenKind.TkComma)
            {
                p.Bump();
                p.Expect(LuaTokenKind.TkName);
            }

            p.Expect(LuaTokenKind.TkGt);
            return m.Complete(p, LuaSyntaxKind.DocGenericDeclareList);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocGenericDeclareList, e.Message);
        }
    }

    private static void AcceptDescription(LuaDocParser p)
    {
        var rollbackPoint = p.GetRollbackPoint();
        if (p.Current is LuaTokenKind.TkDocDescription)
        {
            p.Bump();
            return;
        }
        else if (p.Current is LuaTokenKind.TkEof or LuaTokenKind.TkEndOfLine)
        {
            return;
        }
        p.Rollback(rollbackPoint);
        p.SetState(LuaDocLexerState.Description);
        p.Accept(LuaTokenKind.TkDocDescription);
    }

    private static CompleteMarker TagEnum(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            p.Expect(LuaTokenKind.TkName);
            if (p.Current is LuaTokenKind.TkColon)
            {
                p.Bump();
                TypesParser.Type(p);
            }

            AcceptDescription(p);
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
            p.Expect(LuaTokenKind.TkName);

            if (p.Current is not (LuaTokenKind.TkDocDescription or LuaTokenKind.TkEndOfLine))
            {
                TypesParser.Type(p);
            }

            AcceptDescription(p);
            return m.Complete(p, LuaSyntaxKind.DocAlias);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocAlias, e.Message);
        }
    }

    private static CompleteMarker TagField(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.FieldStart);
        var m = p.Marker();
        p.Bump();
        try
        {
            p.Accept(LuaTokenKind.TkDocVisibility);
            p.SetState(LuaDocLexerState.Normal);
            switch (p.Current)
            {
                case LuaTokenKind.TkLeftBracket:
                {
                    p.Bump();
                    TypesParser.Type(p);
                    p.Expect(LuaTokenKind.TkRightBracket);
                    break;
                }
                case LuaTokenKind.TkName:
                {
                    p.Bump();
                    break;
                }
                default:
                {
                    throw new UnexpectedTokenException("expected <name> or [", p.Current);
                }
            }

            p.Accept(LuaTokenKind.TkNullable);
            TypesParser.Type(p);
            AcceptDescription(p);
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
            AcceptDescription(p);
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
            AcceptDescription(p);
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
            AcceptDescription(p);
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
            var cm = TypesParser.TypedParameter(p);
            while (cm.IsComplete && p.Current is LuaTokenKind.TkComma)
            {
                p.Bump();
                cm = TypesParser.TypedParameter(p);
            }

            AcceptDescription(p);
            return m.Complete(p, LuaSyntaxKind.DocGeneric);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocGeneric, e.Message);
        }
    }

    private static CompleteMarker TagSee(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Description);
        var m = p.Marker();
        p.Bump();
        try
        {
            p.Expect(LuaTokenKind.TkDocDescription);
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
            if (p.CurrentNameText is not "fun")
            {
                throw new UnexpectedTokenException("expected fun", p.Current);
            }

            TypesParser.FunType(p);

            AcceptDescription(p);
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
            AcceptDescription(p);
            return m.Complete(p, LuaSyntaxKind.DocCast);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocCast, e.Message);
        }
    }

    private static CompleteMarker SimpleTag(LuaDocParser p, LuaSyntaxKind kind)
    {
        var m = p.Marker();
        p.Bump();
        AcceptDescription(p);
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
                p.Expect(LuaTokenKind.TkName);
                while (p.Current is LuaTokenKind.TkComma)
                {
                    p.Bump();
                    p.Expect(LuaTokenKind.TkName);
                }
            }

            return m.Complete(p, LuaSyntaxKind.DocDiagnostic);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocDiagnostic, e.Message);
        }
    }

    private static CompleteMarker TagVersion(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            p.Expect(LuaTokenKind.TkName);
            while (p.Current is LuaTokenKind.TkComma)
            {
                p.Bump();
                p.Expect(LuaTokenKind.TkName);
            }

            AcceptDescription(p);
            return m.Complete(p, LuaSyntaxKind.DocVersion);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocVersion, e.Message);
        }
    }

    private static CompleteMarker TagAs(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        TypesParser.Type(p);
        AcceptDescription(p);
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
            AcceptDescription(p);
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
            p.Expect(LuaTokenKind.TkString);
            AcceptDescription(p);
            return m.Complete(p, LuaSyntaxKind.DocModule);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocModule, e.Message);
        }
    }
}
