using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeNameExpr(LuaNameExprSyntax nameExpr)
    {
        declarationContext.ProjectIndex.AddNameExpr(DocumentId, nameExpr);

        var declaration = declarationContext.FindLocalDeclaration(nameExpr);
        if (declaration is not null)
        {
            declarationContext.AddReference(ReferenceKind.Read, declaration, nameExpr);
        }
    }

    private void IndexIndexExpr(LuaIndexExprSyntax indexExpr)
    {
        declarationContext.ProjectIndex.AddIndexExpr(DocumentId, indexExpr);
    }

    private void IndexDocNameType(LuaDocNameTypeSyntax docNameType)
    {
        declarationContext.ProjectIndex.AddNameType(DocumentId, docNameType);
    }
}
