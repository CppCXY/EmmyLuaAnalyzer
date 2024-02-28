using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer;

public abstract class LuaAnalyzer(LuaCompilation compilation)
{
    public LuaCompilation Compilation { get; } = compilation;

    public virtual void Analyze(AnalyzeContext analyzeContext)
    {
    }

    public virtual void RemoveCache(DocumentId documentId)
    {
    }
}
