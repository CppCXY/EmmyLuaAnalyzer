using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private LuaSymbol? AnalyzeDocDetailField(LuaDocFieldSyntax field)
    {
        var visibility = field.Visibility;
        switch (field)
        {
            case { NameField: { } nameField, Type: { } type1 }:
            {
                var type = searchContext.Infer(type1);
                return new LuaSymbol(
                    nameField.RepresentText,
                    type,
                    new DocFieldInfo(new(field)),
                    SymbolFeature.None,
                    GetVisibility(visibility)
                );
            }
            case { IntegerField: { } integerField, Type: { } type2 }:
            {
                var type = searchContext.Infer(type2);
                return new LuaSymbol(
                    $"[{integerField.Value}]",
                    type,
                    new DocFieldInfo( new(field)),
                    SymbolFeature.None,
                    GetVisibility(visibility)
                );
            }
            case { StringField: { } stringField, Type: { } type3 }:
            {
                var type = searchContext.Infer(type3);
                return new LuaSymbol(
                    stringField.Value,
                    type,
                    new DocFieldInfo(new(field)),
                    SymbolFeature.None,
                    GetVisibility(visibility)
                );
            }
        }

        return null;
    }

    private void AnalyzeTypeFields(LuaNamedType namedType, LuaDocTagSyntax typeTag)
    {
        var declarations = new List<LuaSymbol>();
        foreach (var tagField in typeTag.NextOfType<LuaDocTagFieldSyntax>())
        {
            if (tagField.Field is not null)
            {
                if (tagField.Field is { TypeField: { } typeField, Type: { } type4 })
                {
                    var keyType = searchContext.Infer(typeField);
                    var valueType = searchContext.Infer(type4);
                    var docIndexDeclaration = new LuaSymbol(
                        string.Empty,
                        valueType,
                        new TypeIndexInfo(
                            keyType,
                            valueType,
                            new(tagField.Field)
                        ));
                    var indexOperator = new IndexOperator(namedType, keyType, valueType, docIndexDeclaration);
                    declarationContext.TypeManager.AddOperators(namedType, [indexOperator]);
                    continue;
                }

                if (AnalyzeDocDetailField(tagField.Field) is {} declaration)
                {
                    declarations.Add(declaration);
                }
            }
        }

        if (declarations.Count > 0)
        {
            declarationContext.TypeManager.AddMemberDeclarations(namedType, declarations);
        }
    }

    private void AnalyzeTagDocBody(LuaNamedType namedType, LuaDocBodySyntax docBody)
    {
        var declarations = new List<LuaSymbol>();
        foreach (var field in docBody.FieldList)
        {
            if (field is { TypeField: { } typeField, Type: { } type4 })
            {
                var keyType = searchContext.Infer(typeField);
                var valueType = searchContext.Infer(type4);
                var docIndexDeclaration = new LuaSymbol(
                    string.Empty,
                    valueType,
                    new TypeIndexInfo(
                        keyType,
                        valueType,
                        new(field)
                    ));
                var indexOperator = new IndexOperator(namedType, keyType, valueType, docIndexDeclaration);
                declarationContext.TypeManager.AddOperators(namedType, [indexOperator]);
                continue;
            }

            if (AnalyzeDocDetailField(field) is {} declaration)
            {
                declarations.Add(declaration);
            }
        }

        if (declarations.Count > 0)
        {
            declarationContext.TypeManager.AddMemberDeclarations(namedType, declarations);
        }
    }

    private void AnalyzeLuaTableType(LuaDocTableTypeSyntax luaDocTableTypeSyntax)
    {
        var declarations = new List<LuaSymbol>();
        declarationContext.TypeManager.AddElementType(luaDocTableTypeSyntax.UniqueId);
        if (luaDocTableTypeSyntax.Body is not null)
        {
            foreach (var field in luaDocTableTypeSyntax.Body.FieldList)
            {
                if (AnalyzeDocDetailField(field) is {} declaration)
                {
                    declarations.Add(declaration);
                }
            }
        }

        declarationContext.TypeManager.AddElementMembers(luaDocTableTypeSyntax.UniqueId, declarations);
    }

    private void AnalyzeMeta()
    {
        Compilation.Diagnostics.AddMeta(DocumentId);
    }

    public static SymbolVisibility GetVisibility(VisibilityKind visibility)
    {
        return visibility switch
        {
            VisibilityKind.Public => SymbolVisibility.Public,
            VisibilityKind.Protected => SymbolVisibility.Protected,
            VisibilityKind.Private => SymbolVisibility.Private,
            VisibilityKind.Package => SymbolVisibility.Package,
            _ => SymbolVisibility.Public
        };
    }
}
