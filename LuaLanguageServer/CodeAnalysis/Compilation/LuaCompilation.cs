using LuaLanguageServer.CodeAnalysis.Syntax.Diagnostic;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation;

public class LuaCompilation
{
    private LuaWorkspace _workspace;

    private List<LuaSyntaxTree> _syntaxTrees = new();

    public IEnumerable<LuaSyntaxTree> SyntaxTrees => _syntaxTrees;

    public LuaCompilation(LuaWorkspace workspace)
    {
        _workspace = workspace;
    }

    public void AddSyntaxTrees(IEnumerable<LuaSyntaxTree> syntaxTrees)
    {
        _syntaxTrees.AddRange(syntaxTrees);
    }

    public void AddSyntaxTree(LuaSyntaxTree syntaxTree)
    {
        _syntaxTrees.Add(syntaxTree);
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
