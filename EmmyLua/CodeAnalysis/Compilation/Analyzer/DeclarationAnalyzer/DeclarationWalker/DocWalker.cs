using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
     private void AnalyzeDocDetailField(LuaType parentType, LuaDocFieldSyntax field)
    {
        var visibility = field.Visibility;
        switch (field)
        {
            case { NameField: { } nameField, Type: { } type1 }:
            {
                var type = searchContext.Infer(type1);
                var declaration = new LuaDeclaration(
                    nameField.RepresentText,
                    new DocFieldInfo(
                        new(field),
                        type),
                    DeclarationFeature.None,
                    GetVisibility(visibility)
                );
                declarationContext.Db.AddMember(DocumentId, parentType, declaration);
                break;
            }
            case { IntegerField: { } integerField, Type: { } type2 }:
            {
                var type = searchContext.Infer(type2);
                var declaration = new LuaDeclaration(
                    $"[{integerField.Value}]",
                    new DocFieldInfo(
                        new(field),
                        type
                    ),
                    DeclarationFeature.None,
                    GetVisibility(visibility)
                );
                declarationContext.Db.AddMember(DocumentId, parentType, declaration);
                break;
            }
            case { StringField: { } stringField, Type: { } type3 }:
            {
                var type = searchContext.Infer(type3);
                var declaration = new LuaDeclaration(
                    stringField.Value,
                    new DocFieldInfo(
                        new(field),
                        type),
                    DeclarationFeature.None,
                    GetVisibility(visibility)
                );
                declarationContext.Db.AddMember(DocumentId, parentType, declaration);
                break;
            }
            case { TypeField: { } typeField, Type: { } type4 }:
            {
                var keyType = searchContext.Infer(typeField);
                var valueType = searchContext.Infer(type4);
                var docIndexDeclaration = new LuaDeclaration(
                    string.Empty,
                    new TypeIndexInfo(
                        keyType,
                        valueType,
                        new(field)
                    ));
                var indexOperator = new IndexOperator(parentType, keyType, valueType, docIndexDeclaration);
                declarationContext.Db.AddTypeOperator(DocumentId, indexOperator);
                break;
            }
        }
    }

    private void AnalyzeTypeFields(LuaNamedType namedType, LuaDocTagSyntax typeTag)
    {
        foreach (var tagField in typeTag.NextOfType<LuaDocTagFieldSyntax>())
        {
            if (tagField.Field is not null)
            {
                AnalyzeDocDetailField(namedType, tagField.Field);
            }
        }
    }

    private void AnalyzeDocBody(LuaType type, LuaDocBodySyntax docBody)
    {
        foreach (var field in docBody.FieldList)
        {
            AnalyzeDocDetailField(type, field);
        }
    }

    private void AnalyzeLuaTableType(LuaDocTableTypeSyntax luaDocTableTypeSyntax)
    {
        var tableType = new LuaDocTableType(luaDocTableTypeSyntax);
        if (luaDocTableTypeSyntax.Body is not null)
        {
            AnalyzeDocBody(tableType, luaDocTableTypeSyntax.Body);
        }
    }

    private void AnalyzeMeta()
    {
        Compilation.Diagnostics.AddMeta(DocumentId);
    }

    public static DeclarationVisibility GetVisibility(VisibilityKind visibility)
    {
        return visibility switch
        {
            VisibilityKind.Public => DeclarationVisibility.Public,
            VisibilityKind.Protected => DeclarationVisibility.Protected,
            VisibilityKind.Private => DeclarationVisibility.Private,
            VisibilityKind.Package => DeclarationVisibility.Package,
            _ => DeclarationVisibility.Public
        };
    }
}
