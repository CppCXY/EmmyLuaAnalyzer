using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private LuaSymbol? AnalyzeDocDetailField(LuaDocFieldSyntax field)
    {
        var visibility = field.Visibility;
        var readonlyFlag = field.ReadOnly;
        switch (field)
        {
            case { NameField: { } nameField, Type: { } type1 }:
            {
                var symbol = new LuaSymbol(
                    nameField.RepresentText,
                    null,
                    new DocFieldInfo(new(field)),
                    readonlyFlag ? SymbolFeature.Readonly : SymbolFeature.None,
                    GetVisibility(visibility)
                );
                var unResolved =
                    new UnResolvedDocSymbol(symbol, new TypeId(type1.UniqueId), ResolveState.UnResolvedType);
                declarationContext.AddUnResolved(unResolved);
                return symbol;
            }
            case { IntegerField: { } integerField, Type: { } type2 }:
            {
                var symbol = new LuaSymbol(
                    $"[{integerField.Value}]",
                    null,
                    new DocFieldInfo(new(field)),
                    readonlyFlag ? SymbolFeature.Readonly : SymbolFeature.None,
                    GetVisibility(visibility)
                );
                var unResolved =
                    new UnResolvedDocSymbol(symbol, new TypeId(type2.UniqueId), ResolveState.UnResolvedType);
                declarationContext.AddUnResolved(unResolved);
                return symbol;
            }
            case { StringField: { } stringField, Type: { } type3 }:
            {
                var symbol = new LuaSymbol(
                    stringField.Value,
                    null,
                    new DocFieldInfo(new(field)),
                    readonlyFlag ? SymbolFeature.Readonly : SymbolFeature.None,
                    GetVisibility(visibility)
                );
                var unResolved =
                    new UnResolvedDocSymbol(symbol, new TypeId(type3.UniqueId), ResolveState.UnResolvedType);
                declarationContext.AddUnResolved(unResolved);
                return symbol;
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
                if (tagField.Field is { TypeField: { } typeField, Type: { } type, UniqueId: { } id })
                {
                    var unResolved = new UnResolvedDocOperator(
                        namedType,
                        TypeOperatorKind.Index,
                        id,
                        [TypeId.Create(typeField), TypeId.Create(type)],
                        ResolveState.UnResolvedType
                    );
                    declarationContext.AddUnResolved(unResolved);
                    continue;
                }

                if (AnalyzeDocDetailField(tagField.Field) is { } declaration)
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
            if (field is { TypeField: { } typeField, Type: { } type4, UniqueId: { } id })
            {
                var unResolved = new UnResolvedDocOperator(
                    namedType,
                    TypeOperatorKind.Index,
                    id,
                    [TypeId.Create(typeField), TypeId.Create(type4)],
                    ResolveState.UnResolvedType
                );
                declarationContext.AddUnResolved(unResolved);
                continue;
            }

            if (AnalyzeDocDetailField(field) is { } declaration)
            {
                declarations.Add(declaration);
            }
        }

        if (declarations.Count > 0)
        {
            declarationContext.TypeManager.AddMemberDeclarations(namedType, declarations);
        }
    }

    // private void AnalyzeLuaTableType(LuaDocTableTypeSyntax luaDocTableTypeSyntax)
    // {
    //     var declarations = new List<LuaSymbol>();
    //     declarationContext.TypeManager.AddLocalTypeInfo(luaDocTableTypeSyntax.UniqueId);
    //     if (luaDocTableTypeSyntax.Body is not null)
    //     {
    //         foreach (var field in luaDocTableTypeSyntax.Body.FieldList)
    //         {
    //             if (AnalyzeDocDetailField(field) is { } declaration)
    //             {
    //                 declarations.Add(declaration);
    //             }
    //         }
    //     }
    //
    //     declarationContext.TypeManager.AddElementMembers(luaDocTableTypeSyntax.UniqueId, declarations);
    // }

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
