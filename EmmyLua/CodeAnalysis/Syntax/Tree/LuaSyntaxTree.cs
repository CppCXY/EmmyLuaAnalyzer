using System.Collections.Immutable;
using EmmyLua.CodeAnalysis.Compile.Lexer;
using EmmyLua.CodeAnalysis.Compile.Parser;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Binder;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Syntax.Tree;

public class LuaSyntaxTree
{
    public LuaDocument Document { get; }

    public GreenNode GreenRoot { get; }

    public List<Diagnostics.Diagnostic> Diagnostics { get; }

    private LuaSourceSyntax? _root;

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
        var builder = new LuaGreenTreeBuilder(parser);
        var (root, diagnostics) = builder.Build();

        return new LuaSyntaxTree(document, root, diagnostics);
    }

    private LuaSyntaxTree(LuaDocument document, GreenNode root, List<Diagnostics.Diagnostic> diagnostics)
    {
        Document = document;
        GreenRoot = root;
        Diagnostics = diagnostics;
    }

    public LuaSourceSyntax BuildRed()
    {
        var root = SyntaxFactory.CreateSyntax(GreenRoot, this, null, 0) as LuaSourceSyntax;
        var queue = new Queue<LuaSyntaxElement>();
        queue.Enqueue(root!);
        while (queue.Count != 0)
        {
            var node = queue.Dequeue();
            var startOffset = node.Range.StartOffset;
            var childrenElement = new List<LuaSyntaxElement>();
            foreach (var child in node.Green.Children)
            {
                var childNode = SyntaxFactory.CreateSyntax(child, this, node, startOffset);
                childNode.ChildPosition = childrenElement.Count;
                childrenElement.Add(childNode);
                startOffset += child.Length;
                if (child.IsNode)
                {
                    queue.Enqueue(childNode);
                }
            }

            node.ChildrenElements = childrenElement.ToImmutableArray();
        }

        return root!;
    }

    public void Reparse()
    {

    }

    public LuaSourceSyntax SyntaxRoot
    {
        get
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_root is null)
            {
                _root = BuildRed();
                BinderData = BinderAnalysis.Analysis(_root!);
            }

            return _root!;
        }
    }

    public void PushDiagnostic(Diagnostics.Diagnostic diagnostic)
    {
        Diagnostics.Add(diagnostic);
    }
}
