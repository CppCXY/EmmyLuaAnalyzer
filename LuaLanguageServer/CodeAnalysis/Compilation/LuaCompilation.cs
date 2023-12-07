using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Semantic;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Diagnostic;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Workspace;
using Index = LuaLanguageServer.CodeAnalysis.Compilation.StubIndex.Index;

namespace LuaLanguageServer.CodeAnalysis.Compilation;

public class LuaCompilation
{
    public LuaWorkspace Workspace { get; }

    private Dictionary<DocumentId, LuaSyntaxTree> _syntaxTrees = new();

    private Dictionary<DocumentId, DeclarationTree> _declarationTrees = new();

    private Dictionary<DocumentId, SemanticModel> _semanticModels = new();

    public IEnumerable<LuaSyntaxTree> SyntaxTrees => _syntaxTrees.Values;

    private HashSet<DocumentId> _dirtyDocuments = new();

    internal Builtin Builtin { get; } = new();

    public StubIndexImpl StubIndexImpl { get; }

    public SearchContext SearchContext => new(this);

    public LuaCompilation(LuaWorkspace workspace)
    {
        Workspace = workspace;
        StubIndexImpl = new StubIndexImpl(this);
    }

    public void AddSyntaxTrees(IEnumerable<(DocumentId, LuaSyntaxTree)> syntaxTrees)
    {
        foreach (var (documentId, syntaxTree) in syntaxTrees)
        {
            AddSyntaxTree(documentId, syntaxTree);
        }
    }

    public void AddSyntaxTree(DocumentId documentId, LuaSyntaxTree syntaxTree)
    {
        if (_syntaxTrees.TryGetValue(documentId, out var oldSyntaxTree))
        {
            _syntaxTrees[documentId] = syntaxTree;
            _declarationTrees.Remove(documentId);
            Index.RemoveIndex(StubIndexImpl, documentId, oldSyntaxTree);
        }
        else
        {
            _syntaxTrees.Add(documentId, syntaxTree);
        }

        _dirtyDocuments.Add(documentId);
    }

    private void LazyAnalyze()
    {
        if (_dirtyDocuments.Count != 0)
        {
            // analyze declaration
            foreach (var documentId in _dirtyDocuments)
            {
                var syntaxTree = _syntaxTrees[documentId];
                _declarationTrees[documentId] = DeclarationTree.From(syntaxTree);
            }

            // analyze symbols
            foreach (var documentId in _dirtyDocuments)
            {
                var syntaxTree = _syntaxTrees[documentId];
                Index.BuildIndex(StubIndexImpl, documentId, syntaxTree);
            }

            _dirtyDocuments.Clear();
        }
    }

    public SemanticModel? GetSemanticModel(string url)
    {
        LazyAnalyze();
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

    public DeclarationTree? GetDeclarationTree(DocumentId documentId)
    {
        LazyAnalyze();
        return _declarationTrees.TryGetValue(documentId, out var declarationTree) ? declarationTree : null;
    }

    // public IEnumerable<Diagnostic> GetDiagnostics(int baseLine = 0) => _syntaxTrees.SelectMany(
    //     tree => tree.Diagnostics.Select(it => it.WithLocation(
    //         it.Range.ToLocation(tree, baseLine)
    //     ))
    // );
}
