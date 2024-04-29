using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Diagnostics.Checkers;

public class DeprecatedChecker(LuaCompilation compilation)
    : DiagnosticCheckerBase(compilation, [
        DiagnosticCode.Deprecated
    ])
{
    public override void Check(DiagnosticContext context)
    {
        var semanticModel = Compilation.GetSemanticModel(context.Document.Id);
        if (semanticModel is null)
        {
            return;
        }

        var document = context.Document;
        foreach (var node in document.SyntaxTree.SyntaxRoot.Descendants)
        {
            switch (node)
            {
                case LuaNameExprSyntax nameExpr:
                {
                    var declaration = semanticModel.DeclarationTree.FindDeclaration(nameExpr, semanticModel.Context);
                    if (declaration is not null)
                    {
                        CheckDeprecated(context, declaration, nameExpr.Range);
                    }

                    break;
                }
                case LuaIndexExprSyntax indexExpr:
                {
                    var declaration = semanticModel.DeclarationTree.FindDeclaration(indexExpr, semanticModel.Context);
                    if (declaration is not null && indexExpr is { KeyElement.Range: { } range })
                    {
                        CheckDeprecated(context, declaration, range);
                    }

                    break;
                }
                case LuaLocalNameSyntax localName:
                {
                    var declaration = semanticModel.DeclarationTree.FindDeclaration(localName, semanticModel.Context);
                    if (declaration is not null)
                    {
                        CheckDeprecated(context, declaration, localName.Range);
                    }

                    break;
                }
                case LuaTableFieldSyntax tableField:
                {
                    var declaration = semanticModel.DeclarationTree.FindDeclaration(tableField, semanticModel.Context);
                    if (declaration is not null && tableField is { KeyElement.Range: { } range })
                    {
                        CheckDeprecated(context, declaration, range);
                    }

                    break;
                }
            }
        }
    }

    private void CheckDeprecated(DiagnosticContext context, LuaDeclaration declaration, SourceRange range)
    {
        if (declaration.IsDeprecated)
        {
            context.Report(new Diagnostic(
                DiagnosticSeverity.Hint,
                DiagnosticCode.Deprecated,
                "Deprecated",
                range,
                DiagnosticTag.Deprecated
            ));
        }
    }
}
