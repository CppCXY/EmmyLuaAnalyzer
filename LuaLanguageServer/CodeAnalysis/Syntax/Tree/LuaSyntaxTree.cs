using LuaLanguageServer.CodeAnalysis.Compile;
using LuaLanguageServer.CodeAnalysis.Compile.Lexer;
using LuaLanguageServer.CodeAnalysis.Compile.Parser;
using LuaLanguageServer.CodeAnalysis.Compile.Source;
using LuaLanguageServer.CodeAnalysis.Syntax.Binder;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Tree;

public class LuaSyntaxTree
{
    public LuaSource Source { get; }

    public GreenNode GreenRoot { get; }

    public List<Diagnostic.Diagnostic> Diagnostics { get; }

    private LuaSourceSyntax? _root;

    public BinderData? BinderData { get; private set; }

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
        var (root, diagnostics) = builder.Build();

        return new LuaSyntaxTree(source, root, diagnostics);
    }

    private LuaSyntaxTree(LuaSource source, GreenNode root, List<Diagnostic.Diagnostic> diagnostics)
    {
        Source = source;
        GreenRoot = root;
        Diagnostics = diagnostics;
    }

    public LuaSourceSyntax SyntaxRoot
    {
        get
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_root is null)
            {
                _root = SyntaxFactory.CreateSyntax(GreenRoot, this, null) as LuaSourceSyntax;
                BinderData = BinderAnalysis.Analysis(_root!);
            }

            return _root!;
        }
    }

    public void PushDiagnostic(Diagnostic.Diagnostic diagnostic)
    {
        Diagnostics.Add(diagnostic);
    }
}
