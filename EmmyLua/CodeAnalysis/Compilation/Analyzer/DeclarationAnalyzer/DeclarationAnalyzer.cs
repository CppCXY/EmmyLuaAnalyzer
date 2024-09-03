using EmmyLua.CodeAnalysis.Compilation.Analyzer.AttachDocAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Search;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;

public class DeclarationAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Declaration")
{
    public override void Analyze(AnalyzeContext analyzeContext)
    {
        foreach (var document in analyzeContext.LuaDocuments)
        {
            var declarationContext = new DeclarationContext(document, this, analyzeContext);
            var walker = new DeclarationWalker.DeclarationWalker(declarationContext, Compilation);
            document.SyntaxTree.SyntaxRoot.Accept(walker);

            var tree = declarationContext.GetDeclarationTree();
            if (tree is not null)
            {
                Compilation.ProjectIndex.AddDeclarationTree(document.Id, tree);
            }

            analyzeContext.DeclarationContexts.Add(declarationContext);
        }
    }
}
