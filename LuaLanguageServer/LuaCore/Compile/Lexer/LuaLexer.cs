using LuaLanguageServer.LuaCore.Compile.Source;
using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Compile.Lexer;

public class LuaLexer
{
    private LuaSource Source { get; }
    private SourceReader Reader { get; }

    public LuaLexer(LuaSource source)
    {
        Source = source;
        Reader = new SourceReader(source.Text);
    }

    public LuaTokenKind Lex()
    {
        Reader.ResetBuff();

        switch (Reader.CurrentChar)
        {
            case '\n' or '\r':
            {
                return LexNewLine();
            }
            case ' ' or '\t' or '\f' or '\v':
            {
                return LexWhiteSpace();
            }
            case '-':
            {
                Reader.Bump();
                if (Reader.CurrentChar != '-')
                {
                    return LuaTokenKind.TkMinus;
                }

                Reader.Bump();
                if (Reader.CurrentChar == '[')
                {
                    Reader.Bump();
                    var sep = SkipSep();
                    if (Reader.CurrentChar == '[')
                    {
                        Reader.Bump();
                        LexLongString(sep);
                        return LuaTokenKind.TkLongString;
                    }
                }

                Reader.EatWhen(ch => ch != '\n' && ch != '\r');
                return LuaTokenKind.TkShortComment;
            }
            case '[':
            {
                Reader.Bump();
                var sep = SkipSep();
                if (sep <= 0) return LuaTokenKind.TkLeftBracket;
                if (Reader.CurrentChar != '[') return LuaTokenKind.TkUnCompleteLongStringStart;
                Reader.Bump();
                LexLongString(sep);
                return LuaTokenKind.TkLongString;
            }
            case '=':
            {
                Reader.Bump();
                if (Reader.CurrentChar != '=') return LuaTokenKind.TkAssign;
                Reader.Bump();
                return LuaTokenKind.TkEq;
            }
            case '<':
            {
                Reader.Bump();
                if (Reader.CurrentChar != '=') return LuaTokenKind.TkLt;
                Reader.Bump();
                return LuaTokenKind.TkLe;
            }
            case '>':
            {
                Reader.Bump();
                if (Reader.CurrentChar != '=') return LuaTokenKind.TkGt;
                Reader.Bump();
                return LuaTokenKind.TkGe;
            }
            case '~':
            {
                Reader.Bump();
                if (Reader.CurrentChar != '=') return LuaTokenKind.TkBitXor;
                Reader.Bump();
                return LuaTokenKind.TkNe;
            }
            case ':':
            {
                Reader.Bump();
                if (Reader.CurrentChar != ':') return LuaTokenKind.TkColon;
                Reader.Bump();
                return LuaTokenKind.TkDbColon;
            }
            case '"' or '\'':
            {
                var quote = Reader.CurrentChar;
                Reader.Bump();
                Reader.EatWhen(ch =>
                {
                    if (ch == quote || ch is '\n' or '\r') return false;
                    if (ch != '\\') return true;
                    Reader.Bump();
                    switch (Reader.CurrentChar)
                    {
                        // \z will ignore the following whitespace
                        case 'z':
                        {
                            Reader.Bump();
                            Reader.EatWhen(c => c is ' ' or '\t' or '\f' or '\v' or '\r' or '\n');
                            break;
                        }
                        // after \ will ignore the following \n
                        case '\r' or '\n':
                        {
                            LexNewLine();
                            break;
                        }
                        default:
                        {
                            Reader.Bump();
                            break;
                        }
                    }

                    return true;
                });
                if (Reader.CurrentChar != quote) return LuaTokenKind.TkUnFinishedString;
                Reader.Bump();
                return LuaTokenKind.TkString;
            }
            case '.':
            {
                Reader.Bump();
                if (Reader.CurrentChar != '.') return LuaTokenKind.TkDot;
                Reader.Bump();
                if (Reader.CurrentChar != '.') return LuaTokenKind.TkDots;
                Reader.Bump();
                return LuaTokenKind.TkConcat;
            }
            case >= '0' and <= '9':
            {
                return LexNumber();
            }
            default:
                return LuaTokenKind.TkEof;
        }
    }

    private LuaTokenKind LexWhiteSpace()
    {
        Reader.EatWhen(c => c is ' ' or '\t' or '\f' or '\v');
        return LuaTokenKind.TkWhitespace;
    }

    private LuaTokenKind LexNewLine()
    {
        var oldChar = Reader.CurrentChar;
        Reader.Bump();
        // skip \r\n or \n\r
        if ((oldChar == '\r' && Reader.CurrentChar == '\n') ||
            (oldChar == '\n' && Reader.CurrentChar == '\r'))
        {
            Reader.Bump();
        }

        return LuaTokenKind.TkEndOfLine;
    }

    private int SkipSep()
    {
        return Reader.EatWhen(ch => ch == '=');
    }

    private LuaTokenKind LexLongString(int skipSep)
    {
        Reader.EatWhen(ch =>
        {
            if (ch != ']') return true;
            var sep = SkipSep();
            if (sep != skipSep || Reader.CurrentChar != ']') return true;
            Reader.Bump();
            return false;
        });

        return LuaTokenKind.TkLongString;
    }

    private LuaTokenKind LexNumber()
    {
        var first = Reader.CurrentChar;
        Reader.Bump();
        // 解析16进制整数
        if (first == '0' && Reader.CurrentChar is 'x' or 'X') /* hexadecimal? */
        {
            Reader.Bump();
            Reader.EatWhen(ch => ch is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F');
            return LuaTokenKind.TkInt;
        }

        return LuaTokenKind.TkNumber;
    }
}
