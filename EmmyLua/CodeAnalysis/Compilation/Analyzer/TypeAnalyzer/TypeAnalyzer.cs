using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.TypeAnalyzer;

public class TypeAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Type")
{
    public override void Analyze(AnalyzeContext analyzeContext)
    {
        foreach (var document in analyzeContext.LuaDocuments)
        {
            var typeContext = new TypeContext(Compilation);
            var docTypes = document.SyntaxTree.SyntaxRoot.Descendants.OfType<LuaDocTypeSyntax>();
            foreach (var docType in docTypes)
            {
                if (typeContext.IsIgnoreRange(docType.Range))
                {
                    continue;
                }

                typeContext.AddIgnoreRange(docType.Range);
                CompileType(docType, typeContext);
            }

        }
    }

    private LuaType? CompileType(LuaDocTypeSyntax docTypeSyntax, TypeContext context)
    {
        switch (docTypeSyntax)
        {
            case LuaDocNameTypeSyntax nameTypeSyntax:
                CompileNameType(nameType, context);
                break;
        }
    }

}
