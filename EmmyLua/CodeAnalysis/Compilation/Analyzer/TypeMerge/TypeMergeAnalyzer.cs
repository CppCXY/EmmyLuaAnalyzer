using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.TypeMerge;

public class TypeMergeAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    public override void Analyze(DocumentId documentId)
    {
        base.Analyze(documentId);
    }

    public override void RemoveCache(DocumentId documentId)
    {
        base.RemoveCache(documentId);
    }
}
