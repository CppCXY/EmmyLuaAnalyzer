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
                        return LuaTokenKind.TkLongComment;
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
            case SourceReader.Eof:
            {
                return LuaTokenKind.TkEof;
            }
            case '/':
            {
                Reader.Bump();
                if (Reader.CurrentChar != '/') return LuaTokenKind.TkDiv;
                Reader.Bump();
                return LuaTokenKind.TkIDiv;
            }
            case '*':
            {
                Reader.Bump();
                return LuaTokenKind.TkMul;
            }
            case '+':
            {
                Reader.Bump();
                return LuaTokenKind.TkPlus;
            }
            case '%':
            {
                Reader.Bump();
                return LuaTokenKind.TkMod;
            }
            case '^':
            {
                Reader.Bump();
                return LuaTokenKind.TkPow;
            }
            case '#':
            {
                Reader.Bump();
                return LuaTokenKind.TkLen;
            }
            case '&':
            {
                Reader.Bump();
                return LuaTokenKind.TkBitAnd;
            }
            case '|':
            {
                Reader.Bump();
                return LuaTokenKind.TkBitOr;
            }
            case '(':
            {
                Reader.Bump();
                return LuaTokenKind.TkLeftParen;
            }
            case ')':
            {
                Reader.Bump();
                return LuaTokenKind.TkRightParen;
            }
            case '{':
            {
                Reader.Bump();
                return LuaTokenKind.TkLeftBrace;
            }
            case '}':
            {
                Reader.Bump();
                return LuaTokenKind.TkRightBrace;
            }
            case ']':
            {
                Reader.Bump();
                return LuaTokenKind.TkRightBracket;
            }
            case ';':
            {
                Reader.Bump();
                return LuaTokenKind.TkSemiColon;
            }
            // default:
            // {
            //
            // }
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

    private enum NumberState
    {
        Int,
        Float,
        Hex,
        HexFloat,
        WithExpo,
    }

    private LuaTokenKind LexNumber()
    {
        var state = NumberState.Int;
        var first = Reader.CurrentChar;
        Reader.Bump();
        switch (first)
        {
            case '0' when Reader.CurrentChar is 'X' or 'x':
                Reader.Bump();
                state = NumberState.Hex;
                break;
            case '.':
                state = NumberState.Float;
                break;
        }

        Reader.EatWhen(ch =>
        {
            switch (state)
            {
                case NumberState.Int:
                {
                    switch (ch)
                    {
                        case >= '0' and <= '9':
                            return true;
                        case '.':
                            state = NumberState.Float;
                            return true;
                    }

                    if (Reader.CurrentChar is not ('e' or 'E')) return false;
                    if (Reader.CurrentChar is '+' or '-')
                    {
                        Reader.Bump();
                    }

                    state = NumberState.WithExpo;
                    return true;
                }
                case NumberState.Float:
                {
                    if (ch is >= '0' and <= '9') return true;
                    if (Reader.CurrentChar is not ('e' or 'E')) return false;
                    if (Reader.CurrentChar is '+' or '-')
                    {
                        Reader.Bump();
                    }

                    state = NumberState.WithExpo;
                    return true;
                }
                case NumberState.Hex:
                {
                    switch (ch)
                    {
                        case >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F':
                            return true;
                        case '.':
                            state = NumberState.HexFloat;
                            return true;
                    }

                    if (Reader.CurrentChar is not ('P' or 'p')) return false;
                    if (Reader.CurrentChar is '+' or '-')
                    {
                        Reader.Bump();
                    }

                    state = NumberState.WithExpo;
                    return true;
                }
                case NumberState.HexFloat:
                {
                    if (ch is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F')
                    {
                        return true;
                    }

                    if (Reader.CurrentChar is not ('P' or 'p')) return false;
                    if (Reader.CurrentChar is '+' or '-')
                    {
                        Reader.Bump();
                    }

                    state = NumberState.WithExpo;
                    return true;
                }
                case NumberState.WithExpo:
                {
                    return ch is >= '0' and <= '9';
                }
                default:
                {
                    return false;
                }
            }
        });

        // TODO Subdivide the number type
        if (Reader.CurrentChar is 'i')
        {
            Reader.Bump();
            return LuaTokenKind.TkComplex;
        }

        // skip suffix
        // ReSharper disable once InvertIf
        if (state is NumberState.Int or NumberState.Hex)
        {
            Reader.EatWhen(ch => ch is 'u' or 'U' or 'l' or 'L');
            return LuaTokenKind.TkInt;
        }

        return LuaTokenKind.TkNumber;
    }
}
