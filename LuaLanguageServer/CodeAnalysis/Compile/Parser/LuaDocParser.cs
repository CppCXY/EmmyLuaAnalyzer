using LuaLanguageServer.CodeAnalysis.Compile.Grammar.Doc;
using LuaLanguageServer.CodeAnalysis.Compile.Lexer;
using LuaLanguageServer.CodeAnalysis.Compile.Source;
using LuaLanguageServer.CodeAnalysis.Kind;

namespace LuaLanguageServer.CodeAnalysis.Compile.Parser;

/// <summary>
/// backtracking parser
/// </summary>
public class LuaDocParser : IParser
{
    private LuaParser OwnerParser { get; }

    public LuaDocLexer Lexer { get; }

    private LuaTokenData _current;

    private bool _invalid;

    private int _originTokenIndex;

    private List<LuaTokenData> OriginLuaTokenList { get; set; }

    public List<MarkEvent> Events => OwnerParser.Events;

    public Marker Marker() => OwnerParser.Marker();

    public LuaDocParser(LuaParser luaParser)
    {
        OwnerParser = luaParser;
        OriginLuaTokenList = new List<LuaTokenData>();
        Lexer = new LuaDocLexer(luaParser.Lexer.Source);
        _current = new LuaTokenData(LuaTokenKind.TkEof, new SourceRange());
        _invalid = true;
        _originTokenIndex = 0;
    }

    public void Parse(List<LuaTokenData> luaTokenData)
    {
        OriginLuaTokenList.Clear();
        OriginLuaTokenList.AddRange(luaTokenData);
        _originTokenIndex = 0;
        _invalid = true;
        Lexer.State = LuaDocLexerState.Invalid;
        CommentParser.Comment(this);
    }

    private LuaTokenData LexToken()
    {
        var kind = LuaTokenKind.TkEof;
        do
        {
            if (Lexer.Invalid)
            {
                if (_originTokenIndex >= OriginLuaTokenList.Count)
                {
                    break;
                }

                var tokenData = OriginLuaTokenList[_originTokenIndex++];
                if (tokenData.Kind is LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine)
                {
                    return tokenData;
                }

                Lexer.Reset(tokenData);
            }

            kind = Lexer.Lex();
        } while (kind == LuaTokenKind.TkEof);

        return new LuaTokenData(kind, Lexer.Reader.SavedRange);
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

    public void SetState(LuaDocLexerState state)
    {
        Lexer.State = state;
    }

    public LuaTokenKind Current
    {
        get
        {
            // ReSharper disable once InvertIf
            if (_invalid)
            {
                _current = LexToken();
                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (Lexer.State == LuaDocLexerState.Normal)
                {
                    while (_current.Kind is LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine
                           or LuaTokenKind.TkDocContinue)
                    {
                        Events.Add(new MarkEvent.EatToken(_current.Range, _current.Kind));
                        _current = LexToken();
                    }
                }
                else if (Lexer.State == LuaDocLexerState.FieldStart)
                {
                    while (_current.Kind is LuaTokenKind.TkWhitespace)
                    {
                        Events.Add(new MarkEvent.EatToken(_current.Range, _current.Kind));
                        _current = LexToken();
                    }
                }

                _invalid = false;
            }

            return _current.Kind;
        }
    }

    public ReadOnlySpan<char> CurrentNameText =>
        Current is not LuaTokenKind.TkName ? ReadOnlySpan<char>.Empty : Lexer.Reader.CurrentSavedText;

    public struct RollbackPoint
    {
        public int EventPosition { get; set; }
        public int OriginTokenIndex { get; set; }
        public int LexerPosition { get; set; }
        public LuaDocLexerState LexerState { get; set; }
        public LuaTokenData Current { get; set; }
        public bool Invalid { get; set; }

        public bool ReaderIsEof { get; set; }
    }

    public RollbackPoint GetRollbackPoint()
    {
        var rollbackPoint = new RollbackPoint()
        {
            EventPosition = Events.Count - 1,
            OriginTokenIndex = _originTokenIndex - 1,
            LexerPosition = Lexer.Reader.CurrentPosition,
            LexerState = Lexer.State,
            Current = _current,
            Invalid = true,
            ReaderIsEof = Lexer.Reader.IsEof
        };

        // ReSharper disable once InvertIf
        if (!_invalid)
        {
            rollbackPoint.LexerPosition -= rollbackPoint.Current.Range.Length;
            rollbackPoint.ReaderIsEof = false;
        }

        return rollbackPoint;
    }

    public void Rollback(RollbackPoint rollbackPoint)
    {
        Events.RemoveRange(rollbackPoint.EventPosition + 1, Events.Count - rollbackPoint.EventPosition - 1);
        _originTokenIndex = rollbackPoint.OriginTokenIndex;
        var tokenData = OriginLuaTokenList[_originTokenIndex++];
        Lexer.Reset(tokenData);
        Lexer.Reader.CurrentPosition = rollbackPoint.LexerPosition;
        Lexer.Reader.ResetBuff();
        Lexer.State = rollbackPoint.LexerState;
        _current = rollbackPoint.Current;
        _invalid = rollbackPoint.Invalid;
        Lexer.Reader.IsEof = rollbackPoint.ReaderIsEof;
    }
}
