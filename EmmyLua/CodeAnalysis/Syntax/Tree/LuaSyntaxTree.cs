using EmmyLua.CodeAnalysis.Compile.Lexer;
using EmmyLua.CodeAnalysis.Compile.Parser;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree.Binder;
using EmmyLua.CodeAnalysis.Syntax.Tree.Green;
using EmmyLua.CodeAnalysis.Syntax.Tree.Red;
using EmmyLua.CodeAnalysis.Syntax.Tree.Token;

namespace EmmyLua.CodeAnalysis.Syntax.Tree;

public class LuaSyntaxTree
{
    // design for optimization
    private List<RedNode> RedNodes { get; }

    private Dictionary<int, string> StringTokenValues { get; } = new();

    private Dictionary<int, (long, string)> IntegerTokenValues { get; } = new();

    private Dictionary<int, double> NumberTokenValues { get; } = new();

    private Dictionary<int, VersionNumber> VersionNumbers { get; } = new();

    public LuaDocument Document { get; }

    public List<Diagnostic> Diagnostics { get; }

    public LuaSourceSyntax SyntaxRoot => (GetElement(0) as LuaSourceSyntax)!;

    public BinderData? BinderData { get; internal set; }

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
        var redTreeBuilder = new LuaRedTreeBuilder();
        var redNodes = redTreeBuilder.Build(root);
        var syntaxTree = new LuaSyntaxTree(document, redNodes, diagnostics);
        return syntaxTree;
    }

    private LuaSyntaxTree(LuaDocument document, List<RedNode> redNodes, List<Diagnostic> diagnostics)
    {
        Document = document;
        RedNodes = redNodes;
        Diagnostics = diagnostics;
        InitNodes();
    }

    private void InitNodes()
    {
        TokenAnalyzer.Analyze(RedNodes.Count, this);
        BinderAnalyzer.Analyze(SyntaxRoot, this);
    }

    internal LuaSyntaxElement? GetElement(int elementId)
    {
        if (elementId < 0 || elementId >= RedNodes.Count)
        {
            return null;
        }

        return SyntaxFactory.CreateSyntax(elementId, this);
    }

    internal int GetRawKind(int elementId)
    {
        if (elementId < 0 || elementId >= RedNodes.Count)
        {
            return 0;
        }

        return RedNodes[elementId].RawKind;
    }

    internal bool IsNode(int elementId)
    {
        if (elementId < 0 || elementId >= RedNodes.Count)
        {
            return false;
        }

        return RedNodes[elementId].RawKind >> 16 == 1;
    }

    internal LuaTokenKind GetTokenKind(int elementId)
    {
        if (elementId < 0 || elementId >= RedNodes.Count)
        {
            return LuaTokenKind.None;
        }

        var rawKind = RedNodes[elementId].RawKind;
        if (rawKind >> 16 == 2)
        {
            return (LuaTokenKind)(rawKind & 0xFFFF);
        }

        return LuaTokenKind.None;
    }

    internal LuaSyntaxKind GetSyntaxKind(int elementId)
    {
        if (elementId < 0 || elementId >= RedNodes.Count)
        {
            return LuaSyntaxKind.None;
        }

        var rawKind = RedNodes[elementId].RawKind;
        if (rawKind >> 16 == 1)
        {
            return (LuaSyntaxKind)(rawKind & 0xFFFF);
        }

        return LuaSyntaxKind.None;
    }

    internal int GetParent(int elementId)
    {
        if (elementId < 0 || elementId >= RedNodes.Count)
        {
            return -1;
        }

        return RedNodes[elementId].Parent;
    }

    internal SourceRange GetSourceRange(int elementId)
    {
        if (elementId < 0 || elementId >= RedNodes.Count)
        {
            return SourceRange.Empty;
        }

        return RedNodes[elementId].Range;
    }

    internal int GetChildStart(int elementId)
    {
        if (elementId < 0 || elementId >= RedNodes.Count)
        {
            return -1;
        }

        return RedNodes[elementId].ChildStart;
    }

    internal int GetChildEnd(int elementId)
    {
        if (elementId < 0 || elementId >= RedNodes.Count)
        {
            return -1;
        }

        return RedNodes[elementId].ChildEnd;
    }

    internal void PushDiagnostic(Diagnostic diagnostic)
    {
        Diagnostics.Add(diagnostic);
    }

    internal void SetStringTokenValue(int elementId, string value)
    {
        StringTokenValues[elementId] = value;
    }

    internal string GetStringTokenValue(int elementId)
    {
        return StringTokenValues[elementId];
    }

    internal void SetIntegerTokenValue(int elementId, long value, string raw)
    {
        IntegerTokenValues[elementId] = (value, raw);
    }

    internal (long, string) GetIntegerTokenValue(int elementId)
    {
        return IntegerTokenValues[elementId];
    }

    internal void SetNumberTokenValue(int elementId, double value)
    {
        NumberTokenValues[elementId] = value;
    }

    internal double GetNumberTokenValue(int elementId)
    {
        return NumberTokenValues[elementId];
    }

    internal void SetVersionNumber(int elementId, VersionNumber versionNumber)
    {
        VersionNumbers[elementId] = versionNumber;
    }

    internal VersionNumber GetVersionNumber(int elementId)
    {
        return VersionNumbers[elementId];
    }
}
