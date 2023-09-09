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
        throw new NotImplementedException();
    }

    public IEnumerable<Diagnostic> GetDiagnostics(int baseLine = 0) => _syntaxTrees.SelectMany(
        tree => tree.Diagnostics.Select(it => it.WithLocation(
            it.Range.ToLocation(tree, baseLine)
        ))
    );
}
