using System.Diagnostics;
using LuaLanguageServer.LuaCore.Compile.Grammar.Lua;
using LuaLanguageServer.LuaCore.Compile.Lexer;
using LuaLanguageServer.LuaCore.Compile.Parser;
using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Compile.Grammar.Doc;

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
            p.SetState(LuaDocLexerState.Init);

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
                p.SetState(LuaDocLexerState.Trivia);
                p.Accept(LuaTokenKind.TkDocTrivia);
            }
        }
    }
}
