using EmmyLua.CodeAnalysis.Compile.Source;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Kind;

namespace EmmyLua.CodeAnalysis.Compile.Lexer;

public enum LuaDocLexerState
{
    Invalid,
    Init,
    Tag,
    Normal,
    FieldStart,
    Description,
    Trivia,
    See,
    Version
}

public class LuaDocLexer(LuaDocument document)
{
    public LuaDocument Document { get; } = document;

    internal SourceReader Reader { get; } = new(document.Text);

    private LuaTokenKind OriginTokenKind { get; set; }

    public LuaDocLexerState State { get; set; } = LuaDocLexerState.Init;

    public bool Invalid => (State is LuaDocLexerState.Invalid) || Reader.IsEof;

    private static LuaTokenKind ToTag(ReadOnlySpan<char> text)
    {
        return text switch
        {
            "class" => LuaTokenKind.TkTagClass,
            "enum" => LuaTokenKind.TkTagEnum,
            "interface" => LuaTokenKind.TkTagInterface,
            "alias" => LuaTokenKind.TkTagAlias,
            "module" => LuaTokenKind.TkTagModule,
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
            "private" or "protected" or "public" or "package" or "internal" => LuaTokenKind.TkTagVisibility,
            "diagnostic" => LuaTokenKind.TkTagDiagnostic,
            "meta" => LuaTokenKind.TkTagMeta,
            "version" => LuaTokenKind.TkTagVersion,
            "as" => LuaTokenKind.TkTagAs,
            "nodiscard" => LuaTokenKind.TkTagNodiscard,
            "operator" => LuaTokenKind.TkTagOperator,
            "mapping" => LuaTokenKind.TkTagMapping,
            _ => LuaTokenKind.TkTagOther
        };
    }

    private static LuaTokenKind ToVisibilityOrName(ReadOnlySpan<char> text)
    {
        return text switch
        {
            "private" or "protected" or "public" or "package" or "internal" => LuaTokenKind.TkDocVisibility,
            _ => LuaTokenKind.TkName
        };
    }

    public void Reset(LuaTokenData tokenData)
    {
        OriginTokenKind = tokenData.Kind;
        Reader.Reset(tokenData.Range);
    }

    public LuaTokenKind ReLex()
    {
        Reader.RecoverLastState();
        return Lex();
    }

    public LuaTokenKind Lex()
    {
        Reader.ResetBuff();
        if (Reader.IsEof)
        {
            return LuaTokenKind.TkEof;
        }

        if (State is LuaDocLexerState.Invalid)
        {
            State = LuaDocLexerState.Init;
        }

        return State switch
        {
            LuaDocLexerState.Init or LuaDocLexerState.Invalid => LexInit(),
            LuaDocLexerState.Tag => LexTag(),
            LuaDocLexerState.Normal => LexNormal(),
            LuaDocLexerState.Description => LexDescription(),
            LuaDocLexerState.Trivia => LexTrivia(),
            LuaDocLexerState.FieldStart => LexFieldStart(),
            LuaDocLexerState.See => LexSee(),
            LuaDocLexerState.Version => LexVersion(),
            _ => throw new ArgumentOutOfRangeException()
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
                        switch (Reader.CurrentChar)
                        {
                            case '@':
                            {
                                Reader.Bump();
                                return LuaTokenKind.TkDocLongStart;
                            }
                            default:
                            {
                                return LuaTokenKind.TkLongCommentStart;
                            }
                        }
                    }
                    case 3:
                    {
                        Reader.EatWhen(IsDocWhitespace);
                        switch (Reader.CurrentChar)
                        {
                            case '@':
                                Reader.Bump();
                                return LuaTokenKind.TkDocStart;
                            case '|':
                                Reader.Bump();
                                return LuaTokenKind.TkDocEnumField;
                            default:
                                return LuaTokenKind.TkNormalStart;
                        }
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
            case ';':
            {
                Reader.Bump();
                return LuaTokenKind.TkSemicolon;
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
            // support luals '|+' what is this
            case '+':
            {
                Reader.Bump();
                return LuaTokenKind.TkPlus;
            }
            case '-':
            {
                var count = Reader.EatWhen('-');
                switch (count)
                {
                    case 1:
                    {
                        return LuaTokenKind.TkMinus;
                    }
                    case 3:
                    {
                        Reader.EatWhen(IsDocWhitespace);
                        if (Reader.CurrentChar is '@')
                        {
                            Reader.Bump();
                            return LuaTokenKind.TkDocStart;
                        }

                        return LuaTokenKind.TkDocContinue;
                    }
                    default:
                    {
                        return LuaTokenKind.TkDocTrivia;
                    }
                }
            }
            case '=':
            {
                Reader.Bump();
                return LuaTokenKind.TkDocMatch;
            }
            case '#' or '@':
            {
                Reader.EatWhen(_ => true);
                return LuaTokenKind.TkDocDetail;
            }
            case var ch when char.IsDigit(ch):
            {
                Reader.EatWhen(char.IsDigit);
                return LuaTokenKind.TkInt;
            }
            case var del and ('"' or '\''):
            {
                Reader.Bump();
                Reader.EatWhen(ch => ch != del);
                if (Reader.CurrentChar == del)
                {
                    Reader.Bump();
                }

                return LuaTokenKind.TkString;
            }
            case '`':
            {
                Reader.Bump();
                Reader.EatWhen(ch => ch != '`');
                if (Reader.CurrentChar == '`')
                {
                    Reader.Bump();
                }

                return LuaTokenKind.TkTypeTemplate;
            }
            case var ch when LuaLexer.IsNameStart(ch):
            {
                Reader.EatWhen(c =>
                    {
                        if (LuaLexer.IsNameContinue(c))
                        {
                            return true;
                        }
                        // support 'aaa.bbbb' 'aaa-bbbb' 'file*'
                        else if ((c is '.' or '-' or '*') && Reader.NextChar != c)
                        {
                            return true;
                        }

                        return false;
                    });


                return Reader.CurrentSavedText switch
                {
                    "true" or "false" => LuaTokenKind.TkDocBoolean,
                    _ => LuaTokenKind.TkName
                };
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
        if (Reader.IsEof)
        {
            return LuaTokenKind.TkEof;
        }

        switch (Reader.CurrentChar)
        {
            case var ch when IsDocWhitespace(ch):
            {
                Reader.EatWhen(IsDocWhitespace);
                return LuaTokenKind.TkWhitespace;
            }
            case '-':
            {
                if (Reader.CurrentPosition != 0)
                {
                    Reader.EatWhen(_ => true);
                    return LuaTokenKind.TkDocDetail;
                }

                var count = Reader.EatWhen('-');
                switch (count)
                {
                    case 3:
                    {
                        Reader.EatWhen(IsDocWhitespace);
                        if (Reader.CurrentChar is '@')
                        {
                            Reader.Bump();
                            return LuaTokenKind.TkDocStart;
                        }

                        return LuaTokenKind.TkDocContinue;
                    }
                    default:
                    {
                        return LuaTokenKind.TkDocTrivia;
                    }
                }
            }
            default:
            {
                Reader.EatWhen(_ => true);
                return LuaTokenKind.TkDocDetail;
            }
        }
    }

    private LuaTokenKind LexTrivia()
    {
        Reader.EatWhen(_ => true);
        return LuaTokenKind.TkDocTrivia;
    }

    private LuaTokenKind LexFieldStart()
    {
        switch (Reader.CurrentChar)
        {
            case var ch when LuaLexer.IsNameStart(ch):
            {
                Reader.EatWhen(LuaLexer.IsNameContinue);
                return ToVisibilityOrName(Reader.CurrentSavedText);
            }
            default:
            {
                return LexNormal();
            }
        }
    }

    private LuaTokenKind LexSee()
    {
        switch (Reader.CurrentChar)
        {
            case '#':
            {
                Reader.Bump();
                return LuaTokenKind.TkLen;
            }
            default:
            {
                return LexNormal();
            }
        }
    }

    private LuaTokenKind LexVersion()
    {
        switch (Reader.CurrentChar)
        {
            case ',':
            {
                Reader.Bump();
                return LuaTokenKind.TkComma;
            }
            case '>':
            {
                Reader.Bump();
                if (Reader.CurrentChar is '=')
                {
                    Reader.Bump();
                    return LuaTokenKind.TkGe;
                }

                return LuaTokenKind.TkGt;
            }
            case '<':
            {
                Reader.Bump();
                if (Reader.CurrentChar is '=')
                {
                    Reader.Bump();
                    return LuaTokenKind.TkLe;
                }
                return LuaTokenKind.TkLt;
            }
            case var num when char.IsDigit(num):
            {
                Reader.EatWhen(ch => char.IsDigit(ch) || ch == '.');
                return LuaTokenKind.TkVersionNumber;
            }
            case var ch when LuaLexer.IsNameStart(ch):
            {
                Reader.EatWhen(LuaLexer.IsNameContinue);
                return Reader.CurrentSavedText is "JIT" ? LuaTokenKind.TkVersionNumber : LuaTokenKind.TkName;
            }
            case ' ':
            {
                Reader.EatWhen(IsDocWhitespace);
                return LuaTokenKind.TkWhitespace;
            }
            default:
            {
                Reader.EatWhen(_ => true);
                return LuaTokenKind.TkDocTrivia;
            }
        }
    }
}
