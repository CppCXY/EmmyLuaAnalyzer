using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer;

public abstract class LuaAnalyzer(LuaCompilation compilation) : ILuaAnalyzer
{
    public LuaCompilation Compilation { get; } = compilation;

    public virtual void Analyze(DocumentId documentId)
    {
    }

    public virtual void RemoveCache(DocumentId documentId)
    {
    }
}
