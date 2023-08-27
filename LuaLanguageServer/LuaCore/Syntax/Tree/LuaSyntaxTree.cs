using LuaLanguageServer.LuaCore.Compile;
using LuaLanguageServer.LuaCore.Compile.Lexer;
using LuaLanguageServer.LuaCore.Compile.Parser;
using LuaLanguageServer.LuaCore.Compile.Source;
using LuaLanguageServer.LuaCore.Compile.TreeBuilder;
using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Syntax.Tree;

public class LuaSyntaxTree
{
    public LuaSource Source { get; }

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
        LuaParser parser = new LuaParser(new LuaLexer(source));
        parser.Parse();

        var tree = new LuaSyntaxTree(source);

        var builder = new LuaGreenTreeBuilder(tree, parser);
        builder.BuildTree();

        return tree;
    }

    public static  LuaSyntaxTree Create(LuaSource source)
    {
        return Create(source, LuaLanguage.Default);
    }

    internal LuaSyntaxTree(LuaSource source)
    {
        Source = source;
    }
}
