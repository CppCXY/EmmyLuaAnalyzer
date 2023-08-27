using LuaLanguageServer.LuaCore.Compile.Grammar.Lua;
using LuaLanguageServer.LuaCore.Compile.Lexer;
using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Compile.Parser;

public class LuaParser : IMarkerEventContainer
{
    private LuaLexer Lexer { get; }
    private List<LuaTokenData> Tokens { get; set; }

    public List<MarkEvent> Events { get; }

    private LuaTokenKind _current;

    private int _tokenIndex;

    private bool _invalid;

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
        Events.Add(new MarkEvent.EatToken(_tokenIndex, Current));
        _tokenIndex++;
        _invalid = true;
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
            SkipTrivia(ref _tokenIndex);
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
