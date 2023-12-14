using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Bind;

public class BindAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    public override void Analyze(DocumentId documentId)
    {
        throw new NotImplementedException();
    }
}
