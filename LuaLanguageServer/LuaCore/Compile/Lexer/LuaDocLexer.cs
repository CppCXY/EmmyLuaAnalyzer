using LuaLanguageServer.LuaCore.Compile.Source;

namespace LuaLanguageServer.LuaCore.Compile.Lexer;

public class LuaDocLexer
{
    private LuaSource Source { get; }
    private SourceReader Reader { get; }

    public LuaDocLexer(LuaSource source)
    {
        Source = source;
        Reader = new SourceReader(source.Text);
    }

    public void Reset(SourceRange range)
    {
        Reader.Reset(range);
    }

    public void Lex()
    {
        throw new NotImplementedException();
    }
}
