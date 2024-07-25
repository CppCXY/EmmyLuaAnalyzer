using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeTableExpr(LuaTableExprSyntax tableExprSyntax)
    {
        declarationContext.TypeManager.AddElementType(tableExprSyntax.UniqueId);
        var fields = new List<LuaDeclaration>();
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
                fields.Add(declaration);
                var unResolveDeclaration = new UnResolvedSymbol(
                    declaration,
                    new LuaExprRef(value),
                    ResolveState.UnResolvedType
                );
                declarationContext.AddUnResolved(unResolveDeclaration);
            }
        }

        if (fields.Count > 0)
        {
            declarationContext.TypeManager.AddElementMember(tableExprSyntax.UniqueId, fields);
        }
    }
}
