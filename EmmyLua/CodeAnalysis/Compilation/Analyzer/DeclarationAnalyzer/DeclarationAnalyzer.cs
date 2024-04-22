using EmmyLua.CodeAnalysis.Compilation.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;

public class DeclarationAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Declaration")
{
    public SearchContext Context { get; } = new(compilation, new SearchContextFeatures() { Cache = false });

    public override void Analyze(AnalyzeContext analyzeContext)
    {
        foreach (var document in analyzeContext.LuaDocuments)
        {
            var builder = new DeclarationBuilder(document.Id, document.SyntaxTree, this, analyzeContext);
            Compilation.DeclarationTrees[document.Id] = builder.Build();
        }
    }
}
