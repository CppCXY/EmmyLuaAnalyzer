using EmmyLua.CodeAnalysis.Compile.Grammar.Lua;
using EmmyLua.CodeAnalysis.Compile.Lexer;
using EmmyLua.CodeAnalysis.Kind;

namespace EmmyLua.CodeAnalysis.Compile.Parser;

/// <summary>
/// looking ahead parser
/// </summary>
public class LuaParser : IMarkerEventContainer
{
    public LuaLexer Lexer { get; }
    private List<LuaTokenData> Tokens { get; set; }

    public List<MarkEvent> Events { get; }

    private LuaTokenKind _current;

    private int _tokenIndex;

    private bool _invalid;

    private LuaDocParser _docParser;

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

    public Marker Marker()
    {
        var position = Events.Count;
        Events.Add(new MarkEvent.NodeStart(0, LuaSyntaxKind.None));
        return new Marker(position);
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
        if (_tokenIndex < Tokens.Count)
        {
            var range = Tokens[_tokenIndex].Range;
            Events.Add(new MarkEvent.EatToken(range, kind));
        }

        _tokenIndex++;
        _invalid = true;
    }

    // 分析连续的空白/注释
    // 此时注释可能会处于错误的父节点中, 在后续的treeBuilder 再做调整
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
                {
                    lineCount = 0;
                    docTokenData.Add(Tokens[index]);
                    break;
                }
                case LuaTokenKind.TkEndOfLine:
                {
                    lineCount++;
                    if (lineCount >= 2 && docTokenData.Count > 0)
                    {
                        ParseComments(docTokenData);
                        docTokenData.Clear();
                    }

                    goto case LuaTokenKind.TkWhitespace;
                }
                case LuaTokenKind.TkShebang:
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
                    // ReSharper disable once InvertIf
                    if (docTokenData.Count > 0)
                    {
                        ParseComments(docTokenData);
                        docTokenData.Clear();
                    }

                    return;
            }
        }

        // ReSharper disable once InvertIf
        if (docTokenData.Count > 0)
        {
            ParseComments(docTokenData);
            docTokenData.Clear();
        }
    }

    private void ParseComments(List<LuaTokenData> tokenData)
    {
        var afterAddList = new List<LuaTokenData>();
        // 反向遍历tokenData, 剔除里面的空白和行尾
        for (var i = tokenData.Count - 1; i >= 0; i--)
        {
            if (tokenData[i].Kind is LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine)
            {
                afterAddList.Add(tokenData[i]);
                tokenData.RemoveAt(i);
            }
            else
            {
                break;
            }
        }

        _docParser.Parse(tokenData);
        // ReSharper disable once InvertIf
        if (afterAddList.Count != 0)
        {
            // 反向遍历添加到event中
            for (var i = afterAddList.Count - 1; i >= 0; i--)
            {
                Events.Add(new MarkEvent.EatToken(afterAddList[i].Range, afterAddList[i].Kind));
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
