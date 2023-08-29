using LuaLanguageServer.LuaCore.Compile.Grammar.Doc;
using LuaLanguageServer.LuaCore.Compile.Lexer;
using LuaLanguageServer.LuaCore.Compile.Source;
using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Compile.Parser;

public class LuaDocParser : IParser
{
    private LuaParser OwnerParser { get; }

    private LuaDocLexer Lexer { get; }

    private bool LexerInvalid { get; set; }

    private LuaTokenData _current;

    private bool _invalid;

    public LuaDocParser(LuaParser luaParser)
    {
        OwnerParser = luaParser;
        LuaTokenQueue = new Queue<LuaTokenData>();
        LexerInvalid = true;
        Lexer = new LuaDocLexer(luaParser.Lexer.Source);
        _current = new LuaTokenData(LuaTokenKind.TkEof, new SourceRange());
        _invalid = true;
    }

    // public List<LuaTokenData> Tokens { get; }

    private Queue<LuaTokenData> LuaTokenQueue { get; set; }

    public List<MarkEvent> Events => OwnerParser.Events;

    public Marker Marker() => OwnerParser.Marker();

    public void Parse(List<LuaTokenData> luaTokenData)
    {
        LexerInvalid = true;
        LuaTokenQueue.Clear();
        foreach (var token in luaTokenData)
        {
            LuaTokenQueue.Enqueue(token);
        }

        CommentParser.Comment(this);
    }

    private LuaTokenData LexToken()
    {
        var kind = LuaTokenKind.TkEof;
        do
        {
            if (LexerInvalid)
            {
                if (LuaTokenQueue.Any())
                {
                    var tokenData = LuaTokenQueue.Dequeue();
                    if (tokenData.Kind is LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine)
                    {
                        return tokenData;
                    }

                    Lexer.Reset(tokenData);
                    LexerInvalid = false;
                }
                else
                {
                    return new LuaTokenData(LuaTokenKind.TkEof, new SourceRange());
                }
            }

            kind = Lexer.Lex();
            if (kind is LuaTokenKind.TkEof)
            {
                LexerInvalid = true;
            }
        } while (kind == LuaTokenKind.TkEof);

        return new LuaTokenData(kind, Lexer.Reader.SavedRange);
    }

    private void SkipTrivia()
    {
        var tokenData = LexToken();
        while (tokenData.Kind is LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine)
        {
            Events.Add(new MarkEvent.EatToken(tokenData.Range, tokenData.Kind));
            tokenData = LexToken();
        }
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
        Events.Add(new MarkEvent.EatToken(_current.Range, _current.Kind));
        _invalid = true;
    }

    public LuaTokenKind Current
    {
        get
        {
            // ReSharper disable once InvertIf
            if (_invalid)
            {
                SkipTrivia();
                _current = LexToken();
                _invalid = false;
            }

            return _current.Kind;
        }
    }

    public LuaTokenKind LookAhead
    {
        get
        {
            // var index = 0;
            // var tokenData = LexToken();
            // while (tokenData.Kind is LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine)
            // {
            //     index++;
            //     tokenData = LexToken();
            // }
            //
            // return tokenData.Kind;
            throw new NotImplementedException();
        }
    }
}
