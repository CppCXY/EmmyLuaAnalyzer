using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer;

public interface ILuaAnalyzer
{
    public LuaCompilation Compilation { get; }

    public void Analyze(AnalyzeContext analyzeContext);

    public void RemoveCache(LuaDocumentId documentId);
}
