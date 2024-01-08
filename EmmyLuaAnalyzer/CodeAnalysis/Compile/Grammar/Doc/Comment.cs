using System.Diagnostics;
using EmmyLuaAnalyzer.CodeAnalysis.Compile.Lexer;
using EmmyLuaAnalyzer.CodeAnalysis.Compile.Parser;
using EmmyLuaAnalyzer.CodeAnalysis.Kind;

namespace EmmyLuaAnalyzer.CodeAnalysis.Compile.Grammar.Doc;

public static class CommentParser
{
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
                    p.Bump();
                    TagParser.Tag(p);
                    break;
                }
                case LuaTokenKind.TkDocEnumField:
                {
                    p.Bump();
                    TagParser.EnumField(p);
                    break;
                }
                case LuaTokenKind.TkDocLongStart:
                {
                    p.Bump();
                    TagParser.LongDocTag(p);
                    break;
                }
                case LuaTokenKind.TkNormalStart or LuaTokenKind.TkLongCommentStart:
                {
                    p.Bump();
                    p.SetState(LuaDocLexerState.Description);
                    p.Accept(LuaTokenKind.TkDocDescription);
                    break;
                }
                case LuaTokenKind.TkEndOfLine or LuaTokenKind.TkWhitespace or LuaTokenKind.TkDocTrivia:
                {
                    p.Bump();
                    break;
                }
                default:
                {
                    throw new UnreachableException();
                }
            }

            // ReSharper disable once InvertIf
            if (!p.Lexer.Reader.IsEof)
            {
                var rollback = p.GetRollbackPoint();
                p.Rollback(rollback);
                p.SetState(LuaDocLexerState.Trivia);
                p.Accept(LuaTokenKind.TkDocTrivia);
            }

            p.SetState(LuaDocLexerState.Init);
        }
    }
}
