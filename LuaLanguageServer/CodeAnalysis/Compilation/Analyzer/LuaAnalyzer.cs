using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer;

public class LuaAnalyzer
{
    public LuaCompilation Compilation { get; }

    public LuaAnalyzer(LuaCompilation compilation)
    {
        Compilation = compilation;
    }

    public void Analyze(DocumentId documentId, LuaSyntaxTree tree)
    {

    }
}
