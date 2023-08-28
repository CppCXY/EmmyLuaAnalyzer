using LuaLanguageServer.LuaCore.Compile.Lexer;
using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Compile.Parser;

public class LuaDocParser: IParser
{
    private LuaParser InnerParser { get; }

    public LuaDocParser(LuaParser luaParser)
    {
        InnerParser = luaParser;
    }

    public List<LuaTokenData>? Tokens { get; private set; }

    public List<MarkEvent> Events => InnerParser.Events;

    public Marker Marker() => InnerParser.Marker();

    public void Parse()
    {
        throw new NotImplementedException();
    }

    public void Expect(LuaTokenKind kind)
    {
        throw new NotImplementedException();
    }

    public void Accept(LuaTokenKind kind)
    {
        throw new NotImplementedException();
    }

    public void Bump()
    {
        throw new NotImplementedException();
    }

    public LuaTokenKind Current { get; }

    public LuaTokenKind LookAhead { get; }
}
