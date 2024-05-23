namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;

public class DeclarationAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Declaration")
{
    public override void Analyze(AnalyzeContext analyzeContext)
    {
        var declarationTrees = analyzeContext.LuaDocuments.Select(
            it => new DeclarationBuilder(it.Id, it.SyntaxTree, this, analyzeContext).Build()
        );
        foreach (var declarationTree in declarationTrees)
        {
            Compilation.DeclarationTrees[declarationTree.SyntaxTree.Document.Id] = declarationTree;
        }
    }
}
