using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeTableExpr(LuaTableExprSyntax tableExprSyntax)
    {
        var tableClass = new LuaTableLiteralType(tableExprSyntax);
        foreach (var fieldSyntax in tableExprSyntax.FieldList)
        {
            if (fieldSyntax is { Name: { } fieldName, Value: { } value })
            {
                var declaration = new LuaDeclaration(
                    fieldName,
                    new TableFieldInfo(
                        new(fieldSyntax),
                        null
                    ));
                declarationContext.AddAttachedDeclaration(fieldSyntax, declaration);
                declarationContext.Db.AddMember(DocumentId, tableClass, declaration);
                var unResolveDeclaration = new UnResolvedDeclaration(
                    declaration,
                    new LuaExprRef(value),
                    ResolveState.UnResolvedType
                );
                declarationContext.AddUnResolved(unResolveDeclaration);
            }
        }
    }
}
