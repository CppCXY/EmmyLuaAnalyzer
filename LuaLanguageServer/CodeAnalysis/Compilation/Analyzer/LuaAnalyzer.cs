using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.StubIndex;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Workspace;
using Index = LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.StubIndex.Index;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer;

public class LuaAnalyzer(LuaCompilation compilation)
{
    public LuaCompilation Compilation { get; } = compilation;

    private HashSet<DocumentId> DirtyDocuments { get; } = new();

    private Dictionary<DocumentId, DeclarationTree> DeclarationTrees { get; } = new();

    public void Analyze()
    {
        if (DirtyDocuments.Count != 0)
        {
            try
            {
                // analyze declaration
                foreach (var documentId in DirtyDocuments)
                {
                    if (Compilation.GetSyntaxTree(documentId) is { } syntaxTree)
                    {
                        DeclarationTrees[documentId] = DeclarationTreeBuilder.Build(syntaxTree, this);
                    }
                }

                // analyze index
                foreach (var documentId in DirtyDocuments)
                {
                    if (Compilation.GetSyntaxTree(documentId) is { } syntaxTree)
                    {
                        Index.BuildIndex(Compilation.StubIndexImpl, documentId, syntaxTree);
                    }
                }

                // bind analyze
                foreach (var documentId in DirtyDocuments)
                {
                    if (Compilation.GetSyntaxTree(documentId) is { } syntaxTree)
                    {
                        // Index.BuildIndex(Compilation.StubIndexImpl, documentId, syntaxTree);
                    }
                }
            }
            finally
            {
                DirtyDocuments.Clear();
            }
        }
    }

    public void Remove(DocumentId documentId, LuaSyntaxTree tree)
    {
        DeclarationTrees.Remove(documentId);
        Index.RemoveIndex(Compilation.StubIndexImpl, documentId, tree);
    }

    public void AddDirtyDocument(DocumentId documentId)
    {
        DirtyDocuments.Add(documentId);
    }

    public DeclarationTree? GetDeclarationTree(DocumentId documentId)
    {
        return DeclarationTrees.GetValueOrDefault(documentId);
    }
}
