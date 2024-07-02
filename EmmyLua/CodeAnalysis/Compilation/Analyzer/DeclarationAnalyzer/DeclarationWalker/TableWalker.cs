using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private LuaType? FindTableFieldType(LuaTableFieldSyntax fieldSyntax)
    {
        foreach (var comment in fieldSyntax.Comments)
        {
            foreach (var tagSyntax in comment.DocList)
            {
                if (tagSyntax is LuaDocTagTypeSyntax { TypeList: { } typeList })
                {
                    return searchContext.Infer(typeList.FirstOrDefault());
                }
                else if (tagSyntax is LuaDocTagNamedTypeSyntax { Name: { } name })
                {
                    return new LuaNamedType(name.RepresentText);
                }
            }
        }

        return null;
    }

    private void AnalyzeTableExprDeclaration(LuaTableExprSyntax tableExprSyntax)
    {
        var tableClass = new LuaTableLiteralType(tableExprSyntax);
        foreach (var fieldSyntax in tableExprSyntax.FieldList)
        {
            if (fieldSyntax is { Name: { } fieldName, Value: { } value })
            {
                var type = FindTableFieldType(fieldSyntax);
                var declaration = new LuaDeclaration(
                    fieldName,
                    new TableFieldInfo(
                        new(fieldSyntax),
                        type
                    ));
                declarationContext.Db.AddMember(DocumentId, tableClass, declaration);
                if (type == null)
                {
                    var unResolveDeclaration =
                        new UnResolvedDeclaration(declaration, new LuaExprRef(value), ResolveState.UnResolvedType);
                    declarationContext.AddUnResolved(unResolveDeclaration);
                }
            }
        }
    }
}
