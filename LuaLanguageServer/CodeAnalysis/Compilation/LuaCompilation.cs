using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.StubIndex;
using LuaLanguageServer.CodeAnalysis.Compilation.Semantic;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Workspace;
using Index = LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.StubIndex.Index;

namespace LuaLanguageServer.CodeAnalysis.Compilation;

public class LuaCompilation
{
    public LuaWorkspace Workspace { get; }

    private Dictionary<DocumentId, LuaSyntaxTree> _syntaxTrees = new();

    private Dictionary<DocumentId, SemanticModel> _semanticModels = new();

    public IEnumerable<LuaSyntaxTree> SyntaxTrees => _syntaxTrees.Values;

    public Builtin Builtin { get; } = new();

    public StubIndexImpl StubIndexImpl { get; }

    public SearchContext SearchContext { get; }

    public LuaAnalyzer LuaAnalyzer { get; }

    public LuaCompilation(LuaWorkspace workspace)
    {
        Workspace = workspace;
        StubIndexImpl = new StubIndexImpl(this);
        SearchContext = new SearchContext(this);
        LuaAnalyzer = new LuaAnalyzer(this);
    }

    private void InternalAddSyntaxTree(DocumentId documentId, LuaSyntaxTree syntaxTree)
    {
        if (_syntaxTrees.TryGetValue(documentId, out var oldSyntaxTree))
        {
            _syntaxTrees[documentId] = syntaxTree;
            LuaAnalyzer.Remove(documentId, oldSyntaxTree);
        }
        else
        {
            _syntaxTrees.Add(documentId, syntaxTree);
        }

        LuaAnalyzer.AddDirtyDocument(documentId);
    }

    public void AddSyntaxTrees(IEnumerable<(DocumentId, LuaSyntaxTree)> syntaxTrees)
    {
        foreach (var (documentId, syntaxTree) in syntaxTrees)
        {
            InternalAddSyntaxTree(documentId, syntaxTree);
        }

        LuaAnalyzer.Analyze();
    }

    public void AddSyntaxTree(DocumentId documentId, LuaSyntaxTree syntaxTree)
    {
        InternalAddSyntaxTree(documentId, syntaxTree);
        LuaAnalyzer.Analyze();
    }

    public LuaSyntaxTree? GetSyntaxTree(DocumentId documentId)
    {
        return _syntaxTrees.GetValueOrDefault(documentId);
    }

    public SemanticModel? GetSemanticModel(string url)
    {
        var document = Workspace.GetDocument(url);
        if (document is null)
        {
            return null;
        }

        if (_semanticModels.TryGetValue(document.Id, out var semanticModel))
        {
            return semanticModel;
        }

        semanticModel = new SemanticModel(this, document.SyntaxTree);
        _semanticModels.Add(document.Id, semanticModel);
        return semanticModel;
    }

    // public IEnumerable<Diagnostic> GetDiagnostics(int baseLine = 0) => _syntaxTrees.SelectMany(
    //     tree => tree.Diagnostics.Select(it => it.WithLocation(
    //         it.Range.ToLocation(tree, baseLine)
    //     ))
    // );
}
