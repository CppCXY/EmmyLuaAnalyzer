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

    private List<GreenNode> GreenNodes { get; }

    public static LuaSyntaxTree ParseText(string text, LuaLanguage language)
    {
        var source = LuaSource.From(text, language);
        return Create(source);
    }

    public static LuaSyntaxTree ParseText(string text)
    {
        return ParseText(text, LuaLanguage.Default);
    }

    public static LuaSyntaxTree Create(LuaSource source, LuaLanguage language)
    {
        var parser = new LuaParser(new LuaLexer(source));
        parser.Parse();
        var builder = new LuaGreenTreeBuilder(parser);
        builder.BuildTree();
        var greenNodes = builder.GreenNodes;

        return new LuaSyntaxTree(source, greenNodes);
    }

    public static LuaSyntaxTree Create(LuaSource source)
    {
        return Create(source, LuaLanguage.Default);
    }

    internal LuaSyntaxTree(LuaSource source, List<GreenNode> greenNodes)
    {
        Source = source;
        GreenNodes = greenNodes;
    }
}
