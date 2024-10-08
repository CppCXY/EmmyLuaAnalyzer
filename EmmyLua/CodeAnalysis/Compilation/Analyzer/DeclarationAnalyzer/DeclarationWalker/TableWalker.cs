﻿using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private List<SourceRange> TableIgnoreRanges { get; } = new();

    private void AnalyzeTableExpr(LuaTableExprSyntax tableExprSyntax)
    {
        if (IsIgnoreTable(tableExprSyntax))
        {
            return;
        }

        declarationContext.TypeManager.AddDocumentElementType(tableExprSyntax.UniqueId);
        var fields = new List<LuaSymbol>();
        var fieldList = tableExprSyntax.FieldList.ToList();
        for (var i = 0; i < fieldList.Count; i++)
        {
            var fieldSyntax = fieldList[i];
            if (i == 0 && fieldSyntax.IsValue)
            {
                // declarationContext.TypeManager.AddDocumentElementType(fieldSyntax.UniqueId);
                TableIgnoreRanges.Add(tableExprSyntax.Range);
                return;
            }

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

    private bool IsIgnoreTable(LuaTableExprSyntax tableExprSyntax)
    {
        if (TableIgnoreRanges.Count == 0)
        {
            return false;
        }

        var tableRange = tableExprSyntax.Range;
        foreach (var range in TableIgnoreRanges)
        {
            if (range.Intersect(tableRange))
            {
                return true;
            }
        }

        return false;
    }
}
