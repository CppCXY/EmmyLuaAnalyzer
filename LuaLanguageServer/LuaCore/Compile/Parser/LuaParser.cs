using LuaLanguageServer.LuaCore.Compile.Lexer;

namespace LuaLanguageServer.LuaCore.Compile.Parser;

public class LuaParser
{
    private Lexer.LuaLexer Lexer { get; }

    public LuaParser(LuaLexer lexer)
    {
        Lexer = lexer;
    }

    public void Parse()
    {
        // return new LuaSyntaxTree();
    }
}
