using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Bind;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Stub;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.StubIndex;
using LuaLanguageServer.CodeAnalysis.Compilation.Semantic;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation;

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
        Analyzers = new List<ILuaAnalyzer>()
        {
            new DeclarationAnalyzer(this),
            new IndexAnalyzer(this),
            new BindAnalyzer(this)
        };
    }

    private void InternalAddSyntaxTree(DocumentId documentId, LuaSyntaxTree syntaxTree)
    {
        if (_syntaxTrees.TryGetValue(documentId, out var oldSyntaxTree))
        {
            _syntaxTrees[documentId] = syntaxTree;
            foreach (var luaAnalyzer in Analyzers)
            {
                luaAnalyzer.RemoveCache(documentId);
            }
        }
        else
        {
            _syntaxTrees.Add(documentId, syntaxTree);
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

    // public IEnumerable<Diagnostic> GetDiagnostics(int baseLine = 0) => _syntaxTrees.SelectMany(
    //     tree => tree.Diagnostics.Select(it => it.WithLocation(
    //         it.Range.ToLocation(tree, baseLine)
    //     ))
    // );
}
