using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeNameExpr(LuaNameExprSyntax nameExpr)
    {
        builder.ProjectIndex.AddNameExpr(DocumentId, nameExpr);

        var declaration = builder.FindLocalDeclaration(nameExpr);
        if (declaration is not null)
        {
            builder.AddReference(ReferenceKind.Read, declaration, nameExpr);
        }
    }

    private void IndexIndexExpr(LuaIndexExprSyntax indexExpr)
    {
        builder.ProjectIndex.AddIndexExpr(DocumentId, indexExpr);
    }

    private void IndexDocNameType(LuaDocNameTypeSyntax docNameType)
    {
        builder.ProjectIndex.AddNameType(DocumentId, docNameType);
    }
}
