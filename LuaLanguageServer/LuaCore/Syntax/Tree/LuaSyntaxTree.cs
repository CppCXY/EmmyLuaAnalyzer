using LuaLanguageServer.LuaCore.Compile;
using LuaLanguageServer.LuaCore.Compile.Lexer;
using LuaLanguageServer.LuaCore.Compile.Parser;
using LuaLanguageServer.LuaCore.Compile.Source;
using LuaLanguageServer.LuaCore.Kind;
using LuaLanguageServer.LuaCore.Syntax.Green;

namespace LuaLanguageServer.LuaCore.Syntax.Tree;

public class LuaSyntaxTree
{
    public LuaSource Source { get; }

    public GreenNode GreenRoot { get; }

    public static LuaSyntaxTree ParseText(string text, LuaLanguage language)
    {
        var source = LuaSource.From(text, language);
        return Create(source);
    }

    public static LuaSyntaxTree ParseText(string text)
    {
        return ParseText(text, LuaLanguage.Default);
    }

    public static LuaSyntaxTree Create(LuaSource source)
    {
        var parser = new LuaParser(new LuaLexer(source));
        parser.Parse();
        var builder = new LuaGreenTreeBuilder(parser);
        var root = builder.BuildTree();

        return new LuaSyntaxTree(source, root);
    }

    private LuaSyntaxTree(LuaSource source, GreenNode root)
    {
        Source = source;
        GreenRoot = root;
    }
}
