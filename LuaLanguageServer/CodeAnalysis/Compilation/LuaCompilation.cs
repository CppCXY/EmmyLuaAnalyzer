using LuaLanguageServer.CodeAnalysis.Compilation.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
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

    private List<LuaSyntaxTree> _syntaxTrees = new();

    private Dictionary<LuaSyntaxTree, DeclarationTree> _declarationTrees = new();

    public IEnumerable<LuaSyntaxTree> SyntaxTrees => _syntaxTrees;

    internal Builtin Builtin { get; } = new();

    public StubIndexImpl StubIndexImpl { get; }

    public SearchContext SearchContext => new(this);

    public LuaCompilation(LuaWorkspace workspace)
    {
        Workspace = workspace;
        StubIndexImpl = new StubIndexImpl();
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
        _syntaxTrees.Add(syntaxTree);
        Index.BuildIndex(StubIndexImpl, documentId, syntaxTree);
    }

    public SemanticModel GetSemanticModel(LuaSyntaxTree tree)
    {
        return new SemanticModel(this, tree);
    }

    public SemanticModel? GetSemanticModel(string url)
    {
        var document = Workspace.GetDocument(url);
        return document == null ? null : GetSemanticModel(document.SyntaxTree);
    }

    public DeclarationTree GetDeclarationTree(LuaSyntaxTree tree)
    {
        return _declarationTrees.TryGetValue(tree, out var declarationTree)
            ? declarationTree
            : _declarationTrees[tree] = DeclarationTree.From(tree);
    }

    public IEnumerable<Diagnostic> GetDiagnostics(int baseLine = 0) => _syntaxTrees.SelectMany(
        tree => tree.Diagnostics.Select(it => it.WithLocation(
            it.Range.ToLocation(tree, baseLine)
        ))
    );
}
