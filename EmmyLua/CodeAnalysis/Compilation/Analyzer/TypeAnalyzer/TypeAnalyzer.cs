using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.TypeAnalyzer;

public class TypeAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Doc")
{
    public override void Analyze(AnalyzeContext analyzeContext)
    {
        foreach (var document in analyzeContext.LuaDocuments)
        {
            var comments = document.SyntaxTree.SyntaxRoot.Descendants.OfType<LuaCommentSyntax>();
            foreach (var comment in comments)
            {
                AnalyzeComment(comment);
            }
        }
    }

    private void AnalyzeComment(LuaCommentSyntax commentSyntax)
    {
        foreach (var tagDoc in commentSyntax.DocList)
        {
            switch (tagDoc)
            {
                case LuaDocTagClassSyntax classSyntax:
                {
                    AnalyzeClass(classSyntax);
                    break;
                }
            }
        }
    }

    private void AnalyzeClass(LuaDocTagClassSyntax classSyntax)
    {
        if (classSyntax.Name?.RepresentText is not { } className)
        {
            return;
        }

        var classType = new LuaNamedType(classSyntax.DocumentId, className);
        var classTypeInfo = Compilation.TypeManager.FindTypeInfo(classType);
        if (classTypeInfo is null)
        {
            return;
        }

        if (classTypeInfo.TypeCompute is not null)
        {
            return;
        }


    }


}
