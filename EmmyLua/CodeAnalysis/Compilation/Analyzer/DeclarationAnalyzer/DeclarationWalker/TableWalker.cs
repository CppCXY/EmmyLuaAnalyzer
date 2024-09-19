using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private RangeCollection TableIgnoreRanges { get; } = new();

    private void AnalyzeTableExpr(LuaTableExprSyntax tableExprSyntax)
    {
        if (TableIgnoreRanges.Contains(tableExprSyntax.Range.StartOffset))
        {
            return;
        }

        var localTypeInfo = builder.TypeManager.AddLocalTypeInfo(tableExprSyntax.UniqueId);
        if (localTypeInfo is null)
        {
            return;
        }

        var fieldList = tableExprSyntax.FieldList.ToList();
        for (var i = 0; i < fieldList.Count; i++)
        {
            var fieldSyntax = fieldList[i];
            if (i == 0 && fieldSyntax.IsValue)
            {
                // builder.TypeManager.AddLocalTypeInfo(fieldSyntax.UniqueId);
                TableIgnoreRanges.AddRange(tableExprSyntax.Range);
                return;
            }

            if (fieldSyntax is { Name: { } fieldName, Value: { } value })
            {
                var attachedTypes = GetAttachedTypes(fieldSyntax);
                var type = ResolveVariableType(attachedTypes, 0, value, 0);
                var declaration = new LuaSymbol(
                    fieldName,
                    type,
                    new TableFieldInfo(new(fieldSyntax)));
                builder.AddAttachedDeclaration(fieldSyntax, declaration);
                localTypeInfo.AddDeclaration(declaration);
                builder.ProjectIndex.AddTableField(DocumentId, fieldSyntax);
                if (type is not null)
                {
                    var unResolveDeclaration = new UnResolvedSymbol(
                        declaration,
                        new LuaExprRef(value),
                        ResolveState.UnResolvedType
                    );
                    builder.AddUnResolved(unResolveDeclaration);
                }
            }
        }
    }
}
