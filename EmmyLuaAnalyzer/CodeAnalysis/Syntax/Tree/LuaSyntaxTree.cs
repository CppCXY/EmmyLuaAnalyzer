using EmmyLuaAnalyzer.CodeAnalysis.Compile;
using EmmyLuaAnalyzer.CodeAnalysis.Compile.Lexer;
using EmmyLuaAnalyzer.CodeAnalysis.Compile.Parser;
using EmmyLuaAnalyzer.CodeAnalysis.Compile.Source;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Binder;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Green;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLuaAnalyzer.CodeAnalysis.Workspace;

namespace EmmyLuaAnalyzer.CodeAnalysis.Syntax.Tree;

public class LuaSyntaxTree
{
    public LuaSource Source { get; }

    public GreenNode GreenRoot { get; }

    public List<Compile.Diagnostic.Diagnostic> Diagnostics { get; }

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
        var builder = new LuaGreenTreeBuilder(parser);
        var (root, diagnostics) = builder.Build();

        return new LuaSyntaxTree(source, root, diagnostics);
    }

    private LuaSyntaxTree(LuaSource source, GreenNode root, List<Compile.Diagnostic.Diagnostic> diagnostics)
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

    public void PushDiagnostic(Compile.Diagnostic.Diagnostic diagnostic)
    {
        Diagnostics.Add(diagnostic);
    }
}
