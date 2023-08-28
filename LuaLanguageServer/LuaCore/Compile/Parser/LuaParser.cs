using LuaLanguageServer.LuaCore.Compile.Grammar.Lua;
using LuaLanguageServer.LuaCore.Compile.Lexer;
using LuaLanguageServer.LuaCore.Compile.Source;
using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Compile.Parser;

public class LuaParser : IParser
{
    private LuaLexer Lexer { get; }
    private List<LuaTokenData> Tokens { get; set; }

    public List<MarkEvent> Events { get; }

    private LuaTokenKind _current;

    private int _tokenIndex;

    private bool _invalid;

    private LuaDocParser _docParser;

    public Marker Marker()
    {
        var position = Events.Count;
        Events.Add(new MarkEvent.NodeStart(0, LuaSyntaxKind.None));
        return new Marker(position);
    }

    public LuaParser(LuaLexer lexer)
    {
        Lexer = lexer;
        Tokens = new List<LuaTokenData>();
        Events = new List<MarkEvent>();
        _current = LuaTokenKind.TkEof;
        _tokenIndex = 0;
        _invalid = true;
        _docParser = new LuaDocParser(this);
    }

    public void Parse()
    {
        Tokens = Lexer.Tokenize();
        BlockParser.Block(this, true);
    }

    public void Expect(LuaTokenKind kind)
    {
        if (Current != kind)
        {
            throw new UnexpectedTokenException($"expected {kind} but got {Current}", Current);
        }

        Bump();
    }

    public void Accept(LuaTokenKind kind)
    {
        if (Current == kind)
        {
            Bump();
        }
    }

    public void Bump()
    {
        var kind = Current;
        var range = Tokens[_tokenIndex].Range;
        Events.Add(new MarkEvent.EatToken(range, kind));
        _tokenIndex++;
        _invalid = true;
    }

    // 分析连续的空白/注释
    private void ParseTrivia(ref int index)
    {
        var lineCount = 0;
        var docTokenData = new List<LuaTokenData>();
        for (; index < Tokens.Count; index++)
        {
            switch (Tokens[index].Kind)
            {
                case LuaTokenKind.TkShortComment:
                case LuaTokenKind.TkLongComment:
                case LuaTokenKind.TkShebang:
                {
                    docTokenData.Add(Tokens[index]);
                    break;
                }
                case LuaTokenKind.TkEndOfLine:
                {
                    lineCount++;
                    goto case LuaTokenKind.TkWhitespace;
                }
                case LuaTokenKind.TkWhitespace:
                    if (docTokenData.Count == 0)
                    {
                        Events.Add(new MarkEvent.EatToken(Tokens[index].Range, Tokens[index].Kind));
                    }
                    else
                    {
                        docTokenData.Add(Tokens[index]);
                    }

                    break;
                default:
                    return;
            }
        }
    }

    private void SkipTrivia(ref int index)
    {
        for (; index < Tokens.Count; index++)
        {
            if (Tokens[index].Kind is not (LuaTokenKind.TkShortComment or LuaTokenKind.TkLongComment
                or LuaTokenKind.TkShebang or LuaTokenKind.TkEndOfLine or LuaTokenKind.TkWhitespace))
            {
                return;
            }
        }
    }

    public LuaTokenKind Current
    {
        get
        {
            if (!_invalid) return _current;

            _invalid = false;
            ParseTrivia(ref _tokenIndex);
            _current = _tokenIndex < Tokens.Count ? Tokens[_tokenIndex].Kind : LuaTokenKind.TkEof;
            return _current;
        }
    }

    public ReadOnlySpan<char> CurrentName
    {
        get
        {
            if (Current is LuaTokenKind.TkName)
            {
                var tokenData = Tokens[_tokenIndex];
                return Lexer.Source.Text.AsSpan(tokenData.Range.StartOffset, tokenData.Range.Length);
            }
            else
            {
                return "";
            }
        }
    }

    public int CurrentIndex => _tokenIndex;

    public LuaTokenKind LookAhead
    {
        get
        {
            var next = _tokenIndex + 1;
            SkipTrivia(ref next);
            return next >= Tokens.Count ? LuaTokenKind.TkEof : Tokens[next].Kind;
        }
    }
}
