using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer;

public abstract class LuaAnalyzer(LuaCompilation compilation, string name)
{
    public LuaCompilation Compilation { get; } = compilation;

    public string Name { get; } = name;

    public virtual void Analyze(AnalyzeContext analyzeContext)
    {
    }

    public virtual void RemoveCache(LuaDocumentId documentId)
    {
    }
}
