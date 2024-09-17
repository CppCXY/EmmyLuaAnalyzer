namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;

public class DeclarationAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Declaration")
{
    public override void Analyze(AnalyzeContext analyzeContext)
    {
        foreach (var document in analyzeContext.LuaDocuments)
        {
            var builder = new DeclarationBuilder(document, Compilation, analyzeContext);
            var walker = new DeclarationWalker.DeclarationWalker(builder, Compilation);
            document.SyntaxTree.SyntaxRoot.Accept(walker);
            walker.FinishAttachedAnalyze();

            var tree = builder.Build();
            if (tree is not null)
            {
                Compilation.ProjectIndex.AddDeclarationTree(document.Id, tree);
            }
        }
    }
}
