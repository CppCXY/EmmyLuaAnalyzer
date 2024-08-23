using EmmyLua.CodeAnalysis.Compile.Grammar.Doc;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Compile.Lexer;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compile.Parser;

/// <summary>
/// backtracking parser
/// </summary>
public class LuaDocParser(LuaParser luaParser) : IMarkerEventContainer
{
    private LuaParser OwnerParser { get; } = luaParser;

    public LuaDocLexer Lexer { get; } = new(luaParser.Lexer.Document);

    private LuaTokenData _current = new(LuaTokenKind.TkEof, new SourceRange());

    private int _originTokenIndex;

    private List<LuaTokenData> OriginLuaTokenList { get; set; } = [];

    public List<MarkEvent> Events => OwnerParser.Events;

    public Marker Marker() => OwnerParser.Marker();

    public void Parse(List<LuaTokenData> luaTokenData)
    {
        OriginLuaTokenList.Clear();
        OriginLuaTokenList.AddRange(luaTokenData);
        _originTokenIndex = 0;
        Lexer.State = LuaDocLexerState.Invalid;
        CalcCurrent();
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
        CalcCurrent();
    }

    public void SetState(LuaDocLexerState state)
    {
        Lexer.State = state;
    }

    public LuaDocLexerState GetState() => Lexer.State;

    private void CalcCurrent()
    {
        _current = LexToken();
        switch (Lexer.State)
        {
            case LuaDocLexerState.Normal:
            case LuaDocLexerState.Description:
            case LuaDocLexerState.Version:
            {
                while (_current.Kind is LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine
                       or LuaTokenKind.TkDocContinue)
                {
                    Events.Add(new MarkEvent.EatToken(_current.Range, _current.Kind));
                    _current = LexToken();
                }

                break;
            }
            case LuaDocLexerState.FieldStart:
                case LuaDocLexerState.See:
            {
                while (_current.Kind is LuaTokenKind.TkWhitespace)
                {
                    Events.Add(new MarkEvent.EatToken(_current.Range, _current.Kind));
                    _current = LexToken();
                }

                break;
            }
        }
    }

    public void ReCalcCurrent()
    {
        var kind = Lexer.ReLex();
        _current = new LuaTokenData(kind, Lexer.Reader.SavedRange);
    }

    public LuaTokenKind Current => _current.Kind;

    public ReadOnlySpan<char> CurrentNameText =>
        Current is not LuaTokenKind.TkName ? ReadOnlySpan<char>.Empty : Lexer.Reader.CurrentSavedText;

    public struct RollbackPoint
    {
        public int EventPosition { get; set; }
        public int OriginTokenIndex { get; set; }
        public int LexerPosition { get; set; }
        public LuaDocLexerState LexerState { get; set; }
        public LuaTokenData Current { get; set; }

        public bool ReaderIsEof { get; set; }
    }

    public RollbackPoint GetRollbackPoint()
    {
        var rollbackPoint = new RollbackPoint
        {
            EventPosition = Events.Count - 1,
            OriginTokenIndex = _originTokenIndex - 1,
            LexerPosition = Lexer.Reader.CurrentPosition,
            LexerState = Lexer.State,
            Current = _current,
            ReaderIsEof = Lexer.Reader.IsEof
        };

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
        Lexer.Reader.IsEof = rollbackPoint.ReaderIsEof;
    }
}
