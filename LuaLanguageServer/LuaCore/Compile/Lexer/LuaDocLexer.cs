using LuaLanguageServer.LuaCore.Compile.Source;
using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Compile.Lexer;

public enum LuaDocLexerState
{
    Invalid,
    Init,
    Tag,
    Normal,
    Description,
    Trivia
}

public class LuaDocLexer
{
    private LuaSource Source { get; }

    internal SourceReader Reader { get; }

    private LuaTokenKind OriginTokenKind { get; set; }

    public LuaDocLexerState State { get; set; }

    public bool Invalid => (State is LuaDocLexerState.Invalid) || Reader.IsEof;

    private static LuaTokenKind ToTag(ReadOnlySpan<char> text)
    {
        return text switch
        {
            "class" => LuaTokenKind.TkTagClass,
            "enum" => LuaTokenKind.TkTagEnum,
            "interface" => LuaTokenKind.TkTagInterface,
            "alias" => LuaTokenKind.TkTagAlias,

            "field" => LuaTokenKind.TkTagField,
            "type" => LuaTokenKind.TkTagType,
            "param" => LuaTokenKind.TkTagParam,
            "return" => LuaTokenKind.TkTagReturn,
            "generic" => LuaTokenKind.TkTagGeneric,
            "see" => LuaTokenKind.TkTagSee,
            "overload" => LuaTokenKind.TkTagOverload,
            "async" => LuaTokenKind.TkTagAsync,
            "cast" => LuaTokenKind.TkTagCast,
            "deprecated" => LuaTokenKind.TkTagDeprecated,
            "private" or "protected" or "public" or "package" => LuaTokenKind.TkVisibility,
            "diagnostic" => LuaTokenKind.TkDiagnostic,
            "meta" => LuaTokenKind.TkMeta,
            _ => LuaTokenKind.TkTagOther
        };
    }

    public LuaDocLexer(LuaSource source)
    {
        Source = source;
        Reader = new SourceReader(source.Text);
        State = LuaDocLexerState.Init;
    }

    public void Reset(LuaTokenData tokenData)
    {
        OriginTokenKind = tokenData.Kind;
        Reader.Reset(tokenData.Range);
    }

    public LuaTokenKind Lex()
    {
        if (Reader.IsEof)
        {
            return LuaTokenKind.TkEof;
        }

        return State switch
        {
            LuaDocLexerState.Init => LexInit(),
            LuaDocLexerState.Tag => LexTag(),
            LuaDocLexerState.Normal => LexNormal(),
            LuaDocLexerState.Description => LexDescription(),
            LuaDocLexerState.Trivia => LexTrivia()
        };
    }

    private static bool IsDocWhitespace(char ch) =>
        ch is ' ' or '\t' or '\v' or '\f' or '\r' or '\n';

    private LuaTokenKind LexInit()
    {
        switch (Reader.CurrentChar)
        {
            case '-':
            {
                var count = Reader.EatWhen('-');
                switch (count)
                {
                    case 2:
                    {
                        if (OriginTokenKind is not LuaTokenKind.TkLongComment) return LuaTokenKind.TkNormalStart;
                        // 其正确性在luaParser已经验证
                        Reader.Bump(); // [
                        Reader.EatWhen('=');
                        Reader.Bump(); // [
                        return LuaTokenKind.TkDocLongStart;
                    }
                    case 3:
                    {
                        Reader.EatWhen(IsDocWhitespace);
                        if (Reader.CurrentChar is '@')
                        {
                            return LuaTokenKind.TkDocStart;
                        }

                        goto default;
                    }
                    default:
                    {
                        Reader.EatWhen(_ => true);
                        return LuaTokenKind.TkDocTrivia;
                    }
                }
            }
            default:
            {
                Reader.EatWhen(_ => true);
                return LuaTokenKind.TkDocTrivia;
            }
        }
    }

    private LuaTokenKind LexTag()
    {
        switch (Reader.CurrentChar)
        {
            case var ch when IsDocWhitespace(ch):
            {
                Reader.EatWhen(IsDocWhitespace);
                return LuaTokenKind.TkWhitespace;
            }
            case var ch when LuaLexer.IsNameStart(ch):
            {
                Reader.EatWhen(LuaLexer.IsNameContinue);
                return ToTag(Reader.CurrentSavedText);
            }
            default:
            {
                Reader.EatWhen(_ => true);
                return LuaTokenKind.TkDocTrivia;
            }
        }
    }

    private LuaTokenKind LexNormal()
    {
        switch (Reader.CurrentChar)
        {
            case var ch when IsDocWhitespace(ch):
            {
                Reader.EatWhen(IsDocWhitespace);
                return LuaTokenKind.TkWhitespace;
            }
            case ':':
            {
                Reader.Bump();
                return LuaTokenKind.TkColon;
            }
            case '.':
            {
                Reader.Bump();
                // ReSharper disable once InvertIf
                if (Reader.CurrentChar is '.' && Reader.NextChar is '.')
                {
                    Reader.Bump();
                    Reader.Bump();
                    return LuaTokenKind.TkDots;
                }

                return LuaTokenKind.TkDot;
            }
            case ',':
            {
                Reader.Bump();
                return LuaTokenKind.TkComma;
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
            case '[':
            {
                Reader.Bump();
                return LuaTokenKind.TkLeftBracket;
            }
            case ']':
            {
                Reader.Bump();
                return LuaTokenKind.TkRightBracket;
            }
            case '<':
            {
                Reader.Bump();
                return LuaTokenKind.TkLt;
            }
            case '>':
            {
                Reader.Bump();
                return LuaTokenKind.TkGt;
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
            case '|':
            {
                Reader.Bump();
                return LuaTokenKind.TkDocOr;
            }
            case '?':
            {
                Reader.Bump();
                return LuaTokenKind.TkNullable;
            }
            case '-':
            {
                var count = Reader.EatWhen('-');
                return count switch
                {
                    1 => LuaTokenKind.TkMinus,
                    3 => LuaTokenKind.TkDocContinue,
                    _ => LuaTokenKind.TkDocTrivia
                };
            }
            case '#' or '@':
            {
                Reader.EatWhen(_ => true);
                return LuaTokenKind.TkDocDescription;
            }
            case var ch when char.IsDigit(ch):
            {
                Reader.EatWhen(char.IsDigit);
                return LuaTokenKind.TkNumber;
            }
            case var del and ('"' or '\''):
            {
                Reader.EatWhen(ch => ch != del);
                return LuaTokenKind.TkString;
            }
            case var ch when LuaLexer.IsNameStart(ch):
            {
                Reader.EatWhen(c => LuaLexer.IsNameContinue(c) || c == '.');
                return LuaTokenKind.TkName;
            }
            default:
            {
                Reader.EatWhen(_ => true);
                return LuaTokenKind.TkDocTrivia;
            }
        }
    }

    private LuaTokenKind LexDescription()
    {
        Reader.EatWhen(_ => true);
        return LuaTokenKind.TkDocDescription;
    }

    private LuaTokenKind LexTrivia()
    {
        Reader.EatWhen(_ => true);
        return LuaTokenKind.TkDocTrivia;
    }
}
