using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeTableExpr(LuaTableExprSyntax tableExprSyntax)
    {
        declarationContext.TypeManager.AddDocumentElementType(tableExprSyntax.UniqueId);
        var fields = new List<LuaSymbol>();
        foreach (var fieldSyntax in tableExprSyntax.FieldList)
        {
            if (fieldSyntax is { Name: { } fieldName, Value: { } value })
            {
                var declaration = new LuaSymbol(
                    fieldName,
                    null,
                    new TableFieldInfo(new(fieldSyntax)));
                declarationContext.AddAttachedDeclaration(fieldSyntax, declaration);
                fields.Add(declaration);
                var unResolveDeclaration = new UnResolvedSymbol(
                    declaration,
                    new LuaExprRef(value),
                    ResolveState.UnResolvedType
                );
                declarationContext.AddUnResolved(unResolveDeclaration);
                declarationContext.Db.AddTableField(DocumentId, fieldSyntax);
            }
        }

        if (fields.Count > 0)
        {
            declarationContext.TypeManager.AddElementMembers(tableExprSyntax.UniqueId, fields);
        }
    }
}
