using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;

public class DeclarationAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    public List<IndexDeclaration> IndexDeclarations { get; }= new();

    public override void Analyze(DocumentId documentId)
    {
        if (Compilation.GetSyntaxTree(documentId) is { } syntaxTree)
        {
            var builder = new DeclarationBuilder(documentId, syntaxTree, this);
            Compilation.DeclarationTrees[documentId] = builder.Build();
        }

        AnalyzeIndex();
    }

    private void AnalyzeIndex()
    {
        foreach (var declaration in IndexDeclarations)
        {
            // declaration.Analyze(this);

        }

        IndexDeclarations.Clear();
    }

    public override void RemoveCache(DocumentId documentId)
    {
        Compilation.DeclarationTrees.Remove(documentId);
    }
}
