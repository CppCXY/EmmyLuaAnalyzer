using EmmyLua.CodeAnalysis.Compilation.Analyzer;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.TypeAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Index;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Compilation;

public class LuaCompilation
{
    public LuaWorkspace Workspace { get; }

    private readonly Dictionary<LuaDocumentId, LuaSyntaxTree> _syntaxTrees = new();

    public IEnumerable<LuaSyntaxTree> SyntaxTrees => _syntaxTrees.Values;

    public ProjectIndex ProjectIndex { get; }

    private HashSet<LuaDocumentId> DirtyDocuments { get; } = [];

    internal Dictionary<LuaDocumentId, LuaDeclarationTree> DeclarationTrees { get; } = new();

    internal Dictionary<LuaDocumentId, Dictionary<LuaBlockSyntax, ControlFlowGraph>> ControlFlowGraphs { get; } = new();

    private List<LuaAnalyzer> Analyzers { get; }

    public LuaDiagnostics Diagnostics { get; }

    public LuaCompilation(LuaWorkspace workspace)
    {
        Workspace = workspace;
        ProjectIndex = new ProjectIndex(this);
        Analyzers =
        [
            new DeclarationAnalyzer(this),
            new FlowAnalyzer(this),
            new ResolveAnalyzer(this),
            new TypeAnalyzer(this)
        ];
        Diagnostics = new LuaDiagnostics(this);
    }

    private void InternalAddSyntaxTree(LuaDocumentId documentId, LuaSyntaxTree syntaxTree)
    {
        if (!_syntaxTrees.TryAdd(documentId, syntaxTree))
        {
            RemoveCache(documentId);
            _syntaxTrees[documentId] = syntaxTree;
        }

        AddDirtyDocument(documentId);
    }

    public void AddSyntaxTrees(IEnumerable<(LuaDocumentId, LuaSyntaxTree)> syntaxTrees)
    {
        foreach (var (documentId, syntaxTree) in syntaxTrees)
        {
            InternalAddSyntaxTree(documentId, syntaxTree);
        }

        Analyze();
    }

    public void AddSyntaxTree(LuaDocumentId documentId, LuaSyntaxTree syntaxTree)
    {
        InternalAddSyntaxTree(documentId, syntaxTree);
        Analyze();
    }

    public void RemoveSyntaxTree(LuaDocumentId documentId)
    {
        _syntaxTrees.Remove(documentId);
        RemoveCache(documentId);
    }

    public void RemoveCache(LuaDocumentId documentId)
    {
        foreach (var luaAnalyzer in Analyzers)
        {
            luaAnalyzer.RemoveCache(documentId);
        }

        DeclarationTrees.Remove(documentId);
        ProjectIndex.Remove(documentId);
        ControlFlowGraphs.Remove(documentId);
        Diagnostics.RemoveCache(documentId);
    }

    public LuaSyntaxTree? GetSyntaxTree(LuaDocumentId documentId)
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

        var declarationTree = DeclarationTrees.GetValueOrDefault(document.Id);
        if (declarationTree is null)
        {
            return null;
        }

        return new SemanticModel(this, document, declarationTree);
    }

    public SemanticModel? GetSemanticModel(LuaDocumentId documentId)
    {
        var document = Workspace.GetDocument(documentId);
        if (document is null)
        {
            return null;
        }

        var declarationTree = DeclarationTrees.GetValueOrDefault(documentId);
        if (declarationTree is null)
        {
            return null;
        }

        return new SemanticModel(this, document, declarationTree);
    }

    private void Analyze()
    {
        if (DirtyDocuments.Count != 0)
        {
            try
            {
                var list = new List<LuaDocument>();
                foreach (var documentId in DirtyDocuments)
                {
                    var document = Workspace.GetDocument(documentId);
                    if (document is not null && document.Text.Length < Workspace.Features.DontIndexMaxFileSize)
                    {
                        list.Add(document);
                    }
                }

                var analyzeContext = new AnalyzeContext(list);
                foreach (var analyzer in Analyzers)
                {
                    Workspace.Monitor?.OnAnalyzing(analyzer.Name);
                    analyzer.Analyze(analyzeContext);
                }

                foreach (var document in list)
                {
                    Workspace.Monitor?.OnDiagnosticChecking(document.Path, list.Count);
                    Diagnostics.Check(document);
                }
            }
            finally
            {
                DirtyDocuments.Clear();
            }
        }
    }

    private void AddDirtyDocument(LuaDocumentId documentId)
    {
        DirtyDocuments.Add(documentId);
    }

    public LuaDeclarationTree? GetDeclarationTree(LuaDocumentId documentId)
    {
        return DeclarationTrees.GetValueOrDefault(documentId);
    }

    public IEnumerable<Diagnostic> GetDiagnostics() => _syntaxTrees.Values.SelectMany(
        tree => tree.Diagnostics.Select(it => it with
        {
            Location = tree.Document.GetLocation(it.Range)
        })
    );

    public IEnumerable<Diagnostic> GetDiagnostic(LuaDocumentId documentId) =>
        _syntaxTrees.TryGetValue(documentId, out var tree)
            ? tree.Diagnostics.Select(it => it with
            {
                Location = tree.Document.GetLocation(it.Range)
            })
            : Enumerable.Empty<Diagnostic>();

    public ControlFlowGraph? GetControlFlowGraph(LuaBlockSyntax block)
    {
        var documentId = block.Tree.Document.Id;
        if (ControlFlowGraphs.TryGetValue(documentId, out var cfgDict))
        {
            if (cfgDict.TryGetValue(block, out var cfg))
            {
                return cfg;
            }
        }

        return null;
    }
}
