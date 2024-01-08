using EmmyLuaAnalyzer.CodeAnalysis.Workspace;

namespace EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer;

public interface ILuaAnalyzer
{
    public LuaCompilation Compilation { get; }

    public void Analyze(DocumentId documentId);

    public void RemoveCache(DocumentId documentId);
}
