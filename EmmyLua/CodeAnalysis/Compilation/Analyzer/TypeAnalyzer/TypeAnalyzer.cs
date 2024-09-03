using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.TypeAnalyzer;

public class TypeAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Type")
{
    public override void Analyze(AnalyzeContext analyzeContext)
    {
        foreach (var document in analyzeContext.LuaDocuments)
        {
            var comments = document.SyntaxTree.SyntaxRoot.Descendants.OfType<LuaCommentSyntax>();

        }
    }

    private void AnalyzeComment(LuaCommentSyntax commentSyntax)
    {

    }
}
