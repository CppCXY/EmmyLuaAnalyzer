using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Bind;

public class BindAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    public override void Analyze(DocumentId documentId)
    {
        if (Compilation.GetSyntaxTree(documentId) is { } syntaxTree)
        {
            foreach (var node in syntaxTree.SyntaxRoot.Descendants)
            {
                switch (node)
                {
                    case LuaLocalStatSyntax luaLocalStat:
                    {
                        LocalBindAnalysis(luaLocalStat);
                        break;
                    }
                }
            }
        }
    }

    private void LocalBindAnalysis(LuaLocalStatSyntax luaLocalStat)
    {

    }
}
