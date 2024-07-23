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
        var document = context.Document;
        foreach (var node in document.SyntaxTree.SyntaxRoot.Descendants)
        {
            switch (node)
            {
                case LuaNameExprSyntax nameExpr:
                {
                    var declaration = context.SearchContext.FindDeclaration(nameExpr);
                    if (declaration is not null)
                    {
                        CheckDeprecated(context, declaration, nameExpr.Range);
                    }

                    break;
                }
                case LuaIndexExprSyntax indexExpr:
                {
                    var declaration = context.SearchContext.FindDeclaration(indexExpr);
                    if (declaration is not null && indexExpr is { KeyElement.Range: { } range })
                    {
                        CheckDeprecated(context, declaration, range);
                    }

                    break;
                }
                case LuaLocalNameSyntax localName:
                {
                    var declaration = context.SearchContext.FindDeclaration(localName);
                    if (declaration is not null)
                    {
                        CheckDeprecated(context, declaration, localName.Range);
                    }

                    break;
                }
                // case LuaTableFieldSyntax tableField:
                // {
                //     var declaration = context.SearchContext.FindDeclaration(tableField);
                //     if (declaration is not null && tableField is { KeyElement.Range: { } range })
                //     {
                //         CheckDeprecated(context, declaration, range);
                //     }
                //
                //     break;
                // }
            }
        }
    }

    private void CheckDeprecated(DiagnosticContext context, LuaDeclaration luaDeclaration, SourceRange range)
    {
        if (luaDeclaration.IsDeprecated)
        {
            context.Report(
                DiagnosticCode.Deprecated,
                "Deprecated",
                range,
                DiagnosticTag.Deprecated
            );
        }
    }
}
