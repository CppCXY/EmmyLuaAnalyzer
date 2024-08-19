using EmmyLua.CodeAnalysis.Compile.Lexer;
using EmmyLua.CodeAnalysis.Compile.Parser;
using EmmyLua.CodeAnalysis.Syntax.Kind;

namespace EmmyLua.CodeAnalysis.Compile.Grammar.Doc;

public static class Fields
{
    public static CompleteMarker Field(LuaDocParser p, bool inBody)
    {
        var m = p.Marker();
        try
        {
            if (p.Current is LuaTokenKind.TkName)
            {
                p.SetState(LuaDocLexerState.FieldStart);
                p.ReCalcCurrent();
                p.Accept(LuaTokenKind.TkDocVisibility);
                p.SetState(LuaDocLexerState.Normal);
            }

            switch (p.Current)
            {
                case LuaTokenKind.TkLeftBracket:
                {
                    p.Bump();
                    if (p.Current is LuaTokenKind.TkString or LuaTokenKind.TkInt)
                    {
                        p.Bump();
                    }
                    else
                    {
                        TypesParser.Type(p);
                    }

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
            if (inBody)
            {
                p.Expect(LuaTokenKind.TkColon);
            }

            TypesParser.Type(p);

            if (!inBody)
            {
                DescriptionParser.Description(p);
            }

            return m.Complete(p, LuaSyntaxKind.DocDetailField);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocDetailField, e.Message);
        }
    }

    public static CompleteMarker DefineBody(LuaDocParser p)
    {
        var m = p.Marker();
        p.Bump();

        try
        {
            if (p.Current is LuaTokenKind.TkRightBrace)
            {
                p.Bump();
                return m.Complete(p, LuaSyntaxKind.DocBody);
            }

            var cm = Field(p, true);
            while (cm.IsComplete && (p.Current is LuaTokenKind.TkComma or LuaTokenKind.TkSemicolon))
            {
                p.Bump();
                cm = Field(p, true);
            }

            p.Expect(LuaTokenKind.TkRightBrace);

            return m.Complete(p, LuaSyntaxKind.DocBody);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocBody, e.Message);
        }
    }
}
