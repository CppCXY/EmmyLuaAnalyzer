using EmmyLua.CodeAnalysis.Compilation.Analyzer;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.SymbolAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.TypeAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Index;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compile.Diagnostic;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Compilation;

public class LuaCompilation
{
    public LuaWorkspace Workspace { get; }

    private readonly Dictionary<DocumentId, LuaSyntaxTree> _syntaxTrees = new();

    public IEnumerable<LuaSyntaxTree> SyntaxTrees => _syntaxTrees.Values;

    public IEnumerable<Symbol.Symbol> Symbols => SymbolTrees.Values.SelectMany(it => it.Symbols);

    public Builtin Builtin { get; } = new();

    public ProjectIndex ProjectIndex { get; }

    public SearchContext SearchContext { get; }

    private HashSet<DocumentId> DirtyDocuments { get; } = [];

    internal Dictionary<DocumentId, SymbolTree> SymbolTrees { get; } = new();

    internal Dictionary<DocumentId, Dictionary<LuaBlockSyntax, ControlFlowGraph>> ControlFlowGraphs { get; } = new();

    private List<ILuaAnalyzer> Analyzers { get; }

    public LuaCompilation(LuaWorkspace workspace)
    {
        Workspace = workspace;
        ProjectIndex = new ProjectIndex(this);
        SearchContext = new SearchContext(this);
        Analyzers =
        [
            new DeclarationAnalyzer(this),
            new SymbolAnalyzer(this),
            new TypeAnalyzer(this)
        ];
    }

    private void InternalAddSyntaxTree(DocumentId documentId, LuaSyntaxTree syntaxTree)
    {
        if (!_syntaxTrees.TryAdd(documentId, syntaxTree))
        {
            RemoveCache(documentId);
            _syntaxTrees[documentId] = syntaxTree;
        }

        AddDirtyDocument(documentId);
    }

    public void AddSyntaxTrees(IEnumerable<(DocumentId, LuaSyntaxTree)> syntaxTrees)
    {
        foreach (var (documentId, syntaxTree) in syntaxTrees)
        {
            InternalAddSyntaxTree(documentId, syntaxTree);
        }

        Analyze();
    }

    public void AddSyntaxTree(DocumentId documentId, LuaSyntaxTree syntaxTree)
    {
        InternalAddSyntaxTree(documentId, syntaxTree);
        Analyze();
    }

    public void RemoveSyntaxTree(DocumentId documentId)
    {
        _syntaxTrees.Remove(documentId);
        RemoveCache(documentId);
        Analyze();
    }

    public void RemoveCache(DocumentId documentId)
    {
        foreach (var luaAnalyzer in Analyzers)
        {
            luaAnalyzer.RemoveCache(documentId);
        }

        SymbolTrees.Remove(documentId);
        ProjectIndex.Remove(documentId);
        ControlFlowGraphs.Remove(documentId);
    }

    public LuaSyntaxTree? GetSyntaxTree(DocumentId documentId)
    {
        return _syntaxTrees.GetValueOrDefault(documentId);
    }

    public SemanticModel? GetSemanticModel(string url)
    {
        var document = Workspace.GetDocumentByUri(url);
        if (document is null)
        {
            return null;
        }

        return new SemanticModel(this, document);
    }

    private void Analyze()
    {
        SearchContext.ClearCache();
        if (DirtyDocuments.Count != 0)
        {
            try
            {
                foreach (var analyzer in Analyzers)
                {
                    foreach (var documentId in DirtyDocuments)
                    {
                        analyzer.Analyze(documentId);
                    }
                }
            }
            finally
            {
                DirtyDocuments.Clear();
            }
        }
    }

    private void AddDirtyDocument(DocumentId documentId)
    {
        DirtyDocuments.Add(documentId);
    }

    public SymbolTree? GetSymbolTree(DocumentId documentId)
    {
        return SymbolTrees.GetValueOrDefault(documentId);
    }

    public void AddDiagnostic(DocumentId documentId, Diagnostic diagnostic)
    {
        var syntaxTree = GetSyntaxTree(documentId);
        if (syntaxTree is null)
        {
            return;
        }

        syntaxTree.PushDiagnostic(diagnostic);
    }

    public IEnumerable<Diagnostic> GetDiagnostics() => _syntaxTrees.Values.SelectMany(
        tree => tree.Diagnostics.Select(it => it.WithLocation(
            tree.Document.GetLocation(it.Range)
        ))
    );

    public IEnumerable<Diagnostic> GetDiagnostic(DocumentId documentId) =>
        _syntaxTrees.TryGetValue(documentId, out var tree)
            ? tree.Diagnostics.Select(it => it.WithLocation(
                tree.Document.GetLocation(it.Range)
            ))
            : Enumerable.Empty<Diagnostic>();
}
