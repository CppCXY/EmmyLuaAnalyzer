using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer;

public interface ILuaAnalyzer
{
    public LuaCompilation Compilation { get; }

    public void Analyze(DocumentId documentId);

    public void RemoveCache(DocumentId documentId);
}
