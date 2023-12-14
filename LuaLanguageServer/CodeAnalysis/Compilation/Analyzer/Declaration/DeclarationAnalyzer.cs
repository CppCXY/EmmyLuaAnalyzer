using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;

public class DeclarationAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    public override void Analyze(DocumentId documentId)
    {

    }

    public override void RemoveCache(DocumentId documentId)
    {
        Compilation.DeclarationTrees.Remove(documentId);
    }
}
