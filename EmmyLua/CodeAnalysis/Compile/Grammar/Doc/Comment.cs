using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Compile.Lexer;
using EmmyLua.CodeAnalysis.Compile.Parser;

namespace EmmyLua.CodeAnalysis.Compile.Grammar.Doc;

public static class CommentParser
{
    /// <summary>
    /// ---@class AAA
    /// ---@field field1 number
    /// ---@field field2 string
    ///
    /// ---@interface BBB
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public static CompleteMarker Comment(LuaDocParser p)
    {
        var m = p.Marker();

        Docs(p);

        return m.Complete(p, LuaSyntaxKind.Comment);
    }

    private static void Docs(LuaDocParser p)
    {
        while (p.Current is not LuaTokenKind.TkEof)
        {
            switch (p.Current)
            {
                case LuaTokenKind.TkDocStart:
                {
                    p.SetState(LuaDocLexerState.Tag);
                    p.Bump();
                    TagParser.Tag(p);
                    break;
                }
                case LuaTokenKind.TkDocLongStart:
                {
                    p.SetState(LuaDocLexerState.Tag);
                    p.Bump();
                    TagParser.LongDocTag(p);
                    break;
                }
                case LuaTokenKind.TkNormalStart or LuaTokenKind.TkLongCommentStart:
                {
                    p.SetState(LuaDocLexerState.Description);
                    p.Bump();
                    DescriptionParser.Description(p);
                    break;
                }
                default:
                {
                    p.Bump();
                    break;
                }
            }

            // ReSharper disable once InvertIf
            if (!p.Lexer.Reader.IsEof
                && p.Current is not (LuaTokenKind.TkDocStart or LuaTokenKind.TkDocLongStart))
            {
                p.SetState(LuaDocLexerState.Trivia);
                p.ReCalcCurrent();
                p.Accept(LuaTokenKind.TkDocTrivia);
            }

            p.SetState(LuaDocLexerState.Init);
        }
    }
}
