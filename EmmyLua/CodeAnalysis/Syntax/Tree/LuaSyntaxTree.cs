using EmmyLua.CodeAnalysis.Compile.Lexer;
using EmmyLua.CodeAnalysis.Compile.Parser;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Binder;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Syntax.Tree;

public class LuaSyntaxTree
{
    public LuaDocument Document { get; }

    public List<Diagnostic> Diagnostics { get; }

    public LuaSourceSyntax SyntaxRoot { get; private set; } = null!;

    public BinderData? BinderData { get; private set; }

    public static LuaSyntaxTree ParseText(string text, LuaLanguage language)
    {
        var source = LuaDocument.FromText(text, language);
        return Create(source);
    }

    public static LuaSyntaxTree ParseText(string text)
    {
        return ParseText(text, LuaLanguage.Default);
    }

    public static LuaSyntaxTree Create(LuaDocument document)
    {
        var parser = new LuaParser(new LuaLexer(document));
        var greenTreeBuilder = new LuaGreenTreeBuilder(parser);
        var (root, diagnostics) = greenTreeBuilder.Build();
        var syntaxTree = new LuaSyntaxTree(document, diagnostics);
        var redTreeBuilder = new LuaRedTreeBuilder();
        syntaxTree.SyntaxRoot = redTreeBuilder.Build(root, syntaxTree);
        return syntaxTree;
    }

    private LuaSyntaxTree(LuaDocument document, List<Diagnostic> diagnostics)
    {
        Document = document;
        Diagnostics = diagnostics;
    }

    public void PushDiagnostic(Diagnostic diagnostic)
    {
        Diagnostics.Add(diagnostic);
    }
}
