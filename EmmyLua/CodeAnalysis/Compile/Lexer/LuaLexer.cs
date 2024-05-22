using System.Globalization;
using EmmyLua.CodeAnalysis.Compile.Source;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Kind;

namespace EmmyLua.CodeAnalysis.Compile.Lexer;

public class LuaLexer(LuaDocument document)
{
    public LuaDocument Document { get; } = document;
    private SourceReader Reader { get; } = new(document.Text);

    public List<Diagnostic> Diagnostics { get; } = new();

    // 名字开始, 包括unicode
    public static bool IsNameStart(char c)
    {
        // 根据Unicode标准，以下类别的字符可以作为标识符的开头：
        // UppercaseLetter, LowercaseLetter, TitlecaseLetter, ModifierLetter, OtherLetter, LetterNumber
        // 参考：https://www.unicode.org/reports/tr31/#Default_Identifier_Syntax
        return CharUnicodeInfo.GetUnicodeCategory(c) switch
        {
            UnicodeCategory.UppercaseLetter => true,
            UnicodeCategory.LowercaseLetter => true,
            UnicodeCategory.TitlecaseLetter => true,
            UnicodeCategory.ModifierLetter => true,
            UnicodeCategory.OtherLetter => true,
            UnicodeCategory.LetterNumber => true,
            // 下划线也是开头
            UnicodeCategory.ConnectorPunctuation => c == '_',
            _ => false
        };
    }

    public static bool IsNameContinue(char c)
    {
        // 根据Unicode标准，以下类别的字符可以作为标识符的后续部分：
        // UppercaseLetter, LowercaseLetter, TitlecaseLetter, ModifierLetter, OtherLetter, LetterNumber, NonSpacingMark, SpacingCombiningMark, DecimalDigitNumber, ConnectorPunctuation
        // 参考：https://www.unicode.org/reports/tr31/#Default_Identifier_Syntax
        return CharUnicodeInfo.GetUnicodeCategory(c) switch
        {
            UnicodeCategory.UppercaseLetter => true,
            UnicodeCategory.LowercaseLetter => true,
            UnicodeCategory.TitlecaseLetter => true,
            UnicodeCategory.ModifierLetter => true,
            UnicodeCategory.OtherLetter => true,
            UnicodeCategory.LetterNumber => true,
            UnicodeCategory.NonSpacingMark => true,
            UnicodeCategory.SpacingCombiningMark => true,
            UnicodeCategory.DecimalDigitNumber => true,
            UnicodeCategory.ConnectorPunctuation => true,
            _ => false
        };
    }

    private LuaTokenKind NameToKind(ReadOnlySpan<char> word)
    {
        return word switch
        {
            "and" => LuaTokenKind.TkAnd,
            "break" => LuaTokenKind.TkBreak,
            "do" => LuaTokenKind.TkDo,
            "else" => LuaTokenKind.TkElse,
            "elseif" => LuaTokenKind.TkElseIf,
            "end" => LuaTokenKind.TkEnd,
            "false" => LuaTokenKind.TkFalse,
            "for" => LuaTokenKind.TkFor,
            "function" => LuaTokenKind.TkFunction,
            "goto" => Document.Language.LanguageLevel > LuaLanguageLevel.Lua51
                ? LuaTokenKind.TkGoto
                : LuaTokenKind.TkName,
            "if" => LuaTokenKind.TkIf,
            "in" => LuaTokenKind.TkIn,
            "local" => LuaTokenKind.TkLocal,
            "nil" => LuaTokenKind.TkNil,
            "not" => LuaTokenKind.TkNot,
            "or" => LuaTokenKind.TkOr,
            "repeat" => LuaTokenKind.TkRepeat,
            "return" => LuaTokenKind.TkReturn,
            "then" => LuaTokenKind.TkThen,
            "true" => LuaTokenKind.TkTrue,
            "until" => LuaTokenKind.TkUntil,
            "while" => LuaTokenKind.TkWhile,
            _ => LuaTokenKind.TkName
        };
    }

    public List<LuaTokenData> Tokenize()
    {
        var tokens = new List<LuaTokenData>();
        while (!Reader.IsEof)
        {
            var kind = Lex();
            if (kind == LuaTokenKind.TkEof)
            {
                break;
            }

            tokens.Add(new LuaTokenData(kind, Reader.SavedRange));
        }

        return tokens;
    }

    private LuaTokenKind Lex()
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
                if (sep == 0 && Reader.CurrentChar != '[') return LuaTokenKind.TkLeftBracket;
                if (Reader.CurrentChar != '[')
                {
                    Diagnostics.Add(
                        new Diagnostic(DiagnosticSeverity.Error,
                            DiagnosticCode.SyntaxError,
                            "invalid long string delimiter",
                        Reader.SavedRange));
                    return LuaTokenKind.TkLongString;
                }

                Reader.Bump();
                return LexLongString(sep);
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
                switch (Reader.CurrentChar)
                {
                    case '=':
                        Reader.Bump();
                        return LuaTokenKind.TkLe;
                    case '<':
                        Reader.Bump();
                        return LuaTokenKind.TkShl;
                    default:
                        return LuaTokenKind.TkLt;
                }
            }
            case '>':
            {
                Reader.Bump();
                switch (Reader.CurrentChar)
                {
                    case '=':
                        Reader.Bump();
                        return LuaTokenKind.TkGe;
                    case '>':
                        Reader.Bump();
                        return LuaTokenKind.TkShr;
                    default:
                        return LuaTokenKind.TkGt;
                }
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
            case var quote and ('"' or '\''):
            {
                Reader.Bump();
                while (!Reader.IsEof)
                {
                    var ch = Reader.CurrentChar;
                    if (ch == quote || ch is '\n' or '\r')
                    {
                        break;
                    }

                    if (ch != '\\')
                    {
                        Reader.Bump();
                        continue;
                    }

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
                }

                if (Reader.CurrentChar != quote)
                {
                    Diagnostics.Add(new Diagnostic(
                        DiagnosticSeverity.Error,
                        DiagnosticCode.SyntaxError,
                        "unfinished string",
                        new SourceRange(Reader.CurrentPosition, 1)));
                    return LuaTokenKind.TkString;
                }

                Reader.Bump();
                return LuaTokenKind.TkString;
            }
            case '.':
            {
                if (Reader.NextChar is >= '0' and <= '9')
                {
                    return LexNumber();
                }

                Reader.Bump();
                if (Reader.CurrentChar != '.') return LuaTokenKind.TkDot;
                Reader.Bump();
                if (Reader.CurrentChar != '.') return LuaTokenKind.TkConcat;
                Reader.Bump();
                return LuaTokenKind.TkDots;
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
                if (Reader.CurrentChar != '!') return LuaTokenKind.TkLen;
                Reader.EatWhen(ch => ch is not '\n' and not '\r');
                return LuaTokenKind.TkShebang;
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
                return LuaTokenKind.TkSemicolon;
            }
            case ',':
            {
                Reader.Bump();
                return LuaTokenKind.TkComma;
            }
            // invalid lua token, just for postfix completion
            case '@':
            {
                Reader.Bump();
                return LuaTokenKind.TkAt;
            }
            // 所有非数字可见字符包括unicode字符
            case var ch when IsNameStart(ch):
            {
                Reader.Bump();
                Reader.EatWhen(IsNameContinue);
                var name = Reader.CurrentSavedText;
                return NameToKind(name);
            }
            default:
            {
                Reader.Bump();
                return LuaTokenKind.TkUnknown;
            }
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
        while (!Reader.IsEof)
        {
            if (Reader.CurrentChar != ']')
            {
                Reader.Bump();
                continue;
            }

            Reader.Bump();
            var sep = SkipSep();
            if (sep != skipSep || Reader.CurrentChar != ']')
            {
                continue;
            }

            Reader.Bump();
            return LuaTokenKind.TkLongString;
        }

        Diagnostics.Add(new Diagnostic(
            DiagnosticSeverity.Error,
            DiagnosticCode.SyntaxError,
            "unfinished long string or comment",
            new SourceRange(Reader.CurrentPosition, 1)));

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

        return LuaTokenKind.TkFloat;
    }
}
