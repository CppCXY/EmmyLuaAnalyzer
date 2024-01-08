using EmmyLua.CodeAnalysis.Compilation.Analyzer;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.Bind;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.Stub;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compile.Diagnostic;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Compilation;

public class LuaCompilation
{
    public LuaWorkspace Workspace { get; }

    private readonly Dictionary<DocumentId, LuaSyntaxTree> _syntaxTrees = new();

    private Dictionary<DocumentId, SemanticModel> SemanticModels { get; } = new();

    public IEnumerable<LuaSyntaxTree> SyntaxTrees => _syntaxTrees.Values;

    public Builtin Builtin { get; } = new();

    public StubIndexImpl StubIndexImpl { get; }

    public SearchContext SearchContext { get; }

    private HashSet<DocumentId> DirtyDocuments { get; } = [];

    internal Dictionary<DocumentId, DeclarationTree> DeclarationTrees { get; } = new();

    private List<ILuaAnalyzer> Analyzers { get; }

    public LuaCompilation(LuaWorkspace workspace)
    {
        Workspace = workspace;
        StubIndexImpl = new StubIndexImpl(this);
        SearchContext = new SearchContext(this);
        Analyzers =
        [
            new DeclarationAnalyzer(this),
            new BindAnalyzer(this)
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

    public void RemoveCache(DocumentId documentId)
    {
        foreach (var luaAnalyzer in Analyzers)
        {
            luaAnalyzer.RemoveCache(documentId);
        }

        StubIndexImpl.Remove(documentId);
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

        if (SemanticModels.TryGetValue(document.Id, out var semanticModel))
        {
            return semanticModel;
        }

        semanticModel = new SemanticModel(this, document.SyntaxTree);
        SemanticModels.Add(document.Id, semanticModel);
        return semanticModel;
    }

    private void Analyze()
    {
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

    public DeclarationTree? GetDeclarationTree(DocumentId documentId)
    {
        return DeclarationTrees.GetValueOrDefault(documentId);
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
            tree.Source.GetLocation(it.Range)
        ))
    );
}
