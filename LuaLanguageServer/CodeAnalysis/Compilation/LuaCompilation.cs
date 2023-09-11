using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Syntax.Diagnostic;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Workspace;
using Index = LuaLanguageServer.CodeAnalysis.Compilation.StubIndex.Index;

namespace LuaLanguageServer.CodeAnalysis.Compilation;

public class LuaCompilation
{
    private LuaWorkspace _workspace;

    private List<LuaSyntaxTree> _syntaxTrees = new();

    public IEnumerable<LuaSyntaxTree> SyntaxTrees => _syntaxTrees;

    public StubIndexImpl StubIndexImpl { get; }

    public LuaCompilation(LuaWorkspace workspace)
    {
        _workspace = workspace;
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

    // public SemanticModel GetSemanticModel(LuaSyntaxTree tree)
    // {
    //     return new SemanticModel(_workspace, this);
    // }
    //
    // public SemanticModel GetSemanticModel(string url)
    // {
    //     return GetSemanticModel(_syntaxTrees.First(it => it.Source.Url == url));
    // }

    public IEnumerable<Diagnostic> GetDiagnostics(int baseLine = 0) => _syntaxTrees.SelectMany(
        tree => tree.Diagnostics.Select(it => it.WithLocation(
            it.Range.ToLocation(tree, baseLine)
        ))
    );
}
