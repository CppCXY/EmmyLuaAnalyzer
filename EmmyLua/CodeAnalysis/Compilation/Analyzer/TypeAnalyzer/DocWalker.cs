using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.TypeAnalyzer;

public class TypeUtil
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
                    new LuaTypeRef(LuaTypeId.Create(type1)),
                    new DocFieldInfo(new(field)),
                    readonlyFlag ? SymbolFeature.Readonly : SymbolFeature.None,
                    GetVisibility(visibility)
                );
                return symbol;
            }
            case { IntegerField: { } integerField, Type: { } type2 }:
            {
                var symbol = new LuaSymbol(
                    $"[{integerField.Value}]",
                    new LuaTypeRef(LuaTypeId.Create(type2)),
                    new DocFieldInfo(new(field)),
                    readonlyFlag ? SymbolFeature.Readonly : SymbolFeature.None,
                    GetVisibility(visibility)
                );
                return symbol;
            }
            case { StringField: { } stringField, Type: { } type3 }:
            {
                var symbol = new LuaSymbol(
                    stringField.Value,
                    new LuaTypeRef(LuaTypeId.Create(type3)),
                    new DocFieldInfo(new(field)),
                    readonlyFlag ? SymbolFeature.Readonly : SymbolFeature.None,
                    GetVisibility(visibility)
                );
                return symbol;
            }
        }

        return null;
    }

    private void AnalyzeTypeFields(LuaTypeInfo luaTypeInfo, LuaNamedType namedType, LuaDocTagSyntax typeTag)
    {
        foreach (var tagField in typeTag.NextOfType<LuaDocTagFieldSyntax>())
        {
            if (tagField.Field is not null)
            {
                if (tagField.Field is { TypeField: { } typeField, Type: { } type, UniqueId: { } id })
                {
                    var unResolved = new UnResolvedDocOperator(
                        luaTypeInfo,
                        namedType,
                        TypeOperatorKind.Index,
                        id,
                        [LuaTypeId.Create(typeField), LuaTypeId.Create(type)],
                        ResolveState.UnResolvedType
                    );
                    // declarationContext.AddUnResolved(unResolved);
                    continue;
                }

                if (AnalyzeDocDetailField(tagField.Field) is { } fieldSymbol)
                {
                    luaTypeInfo.AddDeclaration(fieldSymbol);
                }
            }
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
