using EmmyLua.CodeAnalysis.Compilation.Search;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;

public class DeclarationAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Declaration")
{
    public override void Analyze(AnalyzeContext analyzeContext)
    {
        var searchContext = new SearchContext(Compilation, new SearchContextFeatures() { Cache = false });
        foreach (var document in analyzeContext.LuaDocuments)
        {
            var declarationContext = new DeclarationContext(document, this, analyzeContext);
            var walker = new DeclarationWalker.DeclarationWalker(declarationContext, searchContext);
            document.SyntaxTree.SyntaxRoot.Accept(walker);

            var tree = declarationContext.GetDeclarationTree();
            if (tree is not null)
            {
                Compilation.Db.AddDeclarationTree(document.Id, tree);
            }

            var attachDeclarationAnalyzer = new AttachDeclarationAnalyzer(declarationContext, searchContext);
            attachDeclarationAnalyzer.Analyze();
        }
    }
}
