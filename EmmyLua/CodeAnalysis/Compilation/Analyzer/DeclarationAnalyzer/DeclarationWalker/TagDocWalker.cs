using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeTagClass(LuaDocTagClassSyntax tagClassSyntax)
    {
        if (tagClassSyntax is { Name: { } name })
        {
            var attribute = GetAttribute(tagClassSyntax);


            var typeInfo = builder.TypeManager.AddTypeDefinition(
                tagClassSyntax,
                name.RepresentText,
                NamedTypeKind.Class,
                attribute);
            if (typeInfo is null)
            {
                builder.AddDiagnostic(new Diagnostic(
                    DiagnosticSeverity.Error,
                    DiagnosticCode.DuplicateType,
                    $"Type '{name.RepresentText}' already exists",
                    name.Range
                ));
                return;
            }

            var luaClass = new LuaNamedType(DocumentId, name.RepresentText);
            if (tagClassSyntax.GenericDeclareList is { } genericDeclareList)
            {
                AnalyzeTypeGenericParam(typeInfo, genericDeclareList);
            }

            if (tagClassSyntax.ExtendTypeList is { } extendList)
            {
                AnalyzeTypeSupers(typeInfo, extendList);
            }

            if (tagClassSyntax.Body is { } body)
            {
                AnalyzeTagDocBody(typeInfo, body);
            }

            AnalyzeTypeFields(typeInfo, tagClassSyntax);
            AnalyzeTypeOperator(typeInfo, luaClass, tagClassSyntax);
            AttachTypeToNext(luaClass, tagClassSyntax);
        }
    }

    private void AnalyzeTagAlias(LuaDocTagAliasSyntax tagAliasSyntax)
    {
        if (tagAliasSyntax is { Name: { } name })
        {
            var typeInfo = builder.TypeManager.AddTypeComputer(tagAliasSyntax, name.RepresentText);
            if (typeInfo is null)
            {
                builder.AddDiagnostic(new Diagnostic(
                    DiagnosticSeverity.Error,
                    DiagnosticCode.DuplicateType,
                    $"Type '{name.RepresentText}' already exists",
                    name.Range
                ));
            }
        }
    }

    private void AnalyzeTagEnum(LuaDocTagEnumSyntax tagEnumSyntax)
    {
        if (tagEnumSyntax is { Name: { } name })
        {
            var attribute = GetAttribute(tagEnumSyntax);
            var typeInfo = builder.TypeManager.AddTypeDefinition(
                tagEnumSyntax,
                name.RepresentText,
                NamedTypeKind.Enum,
                attribute);

            if (typeInfo is null)
            {
                builder.AddDiagnostic(new Diagnostic(
                    DiagnosticSeverity.Error,
                    DiagnosticCode.DuplicateType,
                    $"Type '{name.RepresentText}' already exists",
                    name.Range
                ));
                return;
            }

            foreach (var field in tagEnumSyntax.FieldList)
            {
                if (field is { Name: { } fieldName })
                {
                    var fieldDeclaration = new LuaSymbol(
                        fieldName.RepresentText,
                        null,
                        new EnumFieldInfo(new(field)));

                    typeInfo.AddDeclaration(fieldDeclaration);
                }
            }

            var luaEnum = new LuaNamedType(DocumentId, name.RepresentText);
            AttachTypeToNext(luaEnum, tagEnumSyntax);
        }
    }

    private void AnalyzeTagInterface(LuaDocTagInterfaceSyntax tagInterfaceSyntax)
    {
        if (tagInterfaceSyntax is { Name: { } name })
        {
            var attribute = GetAttribute(tagInterfaceSyntax);
            var typeInfo = builder.TypeManager.AddTypeDefinition(
                tagInterfaceSyntax,
                name.RepresentText,
                NamedTypeKind.Interface,
                attribute);

            if (typeInfo is null)
            {
                builder.AddDiagnostic(new Diagnostic(
                    DiagnosticSeverity.Error,
                    DiagnosticCode.DuplicateType,
                    $"Type '{name.RepresentText}' already exists",
                    name.Range
                ));
                return;
            }

            var luaInterface = new LuaNamedType(DocumentId, name.RepresentText);
            if (tagInterfaceSyntax.GenericDeclareList is { } genericDeclareList)
            {
                AnalyzeTypeGenericParam(typeInfo, genericDeclareList);
            }

            if (tagInterfaceSyntax.ExtendTypeList is { } extendList)
            {
                AnalyzeTypeSupers(typeInfo, extendList);
            }

            if (tagInterfaceSyntax.Body is { } body)
            {
                AnalyzeTagDocBody(typeInfo, body);
            }

            AnalyzeTypeFields(typeInfo, tagInterfaceSyntax);
            AnalyzeTypeOperator(typeInfo, luaInterface, tagInterfaceSyntax);
            AttachTypeToNext(luaInterface, tagInterfaceSyntax);
        }
    }

    private void AnalyzeTagType(LuaDocTagTypeSyntax tagTypeSyntax)
    {
        foreach (var typeSyntax in tagTypeSyntax.TypeList)
        {
            var luaType = builder.CreateRef(typeSyntax);
            AttachTypeToNext(luaType, tagTypeSyntax);
        }
    }

    private void AnalyzeTagModule(LuaDocTagModuleSyntax moduleSyntax)
    {
        if (moduleSyntax.Module is { Value: { } moduleName })
        {
            Compilation.Project.ModuleManager.AddVirtualModule(DocumentId, moduleName);
        }
        else if (moduleSyntax.Action is { Text: "no-require" })
        {
            Compilation.Project.ModuleManager.AddDisableRequire(DocumentId);
        }
    }

    private void AnalyzeTagDiagnostic(LuaDocTagDiagnosticSyntax diagnosticSyntax)
    {
        if (diagnosticSyntax is
            {
                Action: { Text: { } actionName },
                Diagnostics: { DiagnosticNames: { } diagnosticNames }
            })
        {
            switch (actionName)
            {
                case "disable-next-line":
                {
                    if (diagnosticSyntax.Parent is LuaCommentSyntax { Owner.Range: { } range })
                    {
                        foreach (var diagnosticName in diagnosticNames)
                        {
                            if (diagnosticName is { RepresentText: { } name })
                            {
                                Compilation.Diagnostics.AddDiagnosticDisableNextLine(DocumentId, range, name);
                            }
                        }
                    }

                    break;
                }
                case "disable":
                {
                    foreach (var diagnosticName in diagnosticNames)
                    {
                        if (diagnosticName is { RepresentText: { } name })
                        {
                            Compilation.Diagnostics.AddDiagnosticDisable(DocumentId, name);
                        }
                    }

                    break;
                }
                case "enable":
                {
                    foreach (var diagnosticName in diagnosticNames)
                    {
                        if (diagnosticName is { RepresentText: { } name })
                        {
                            Compilation.Diagnostics.AddDiagnosticEnable(DocumentId, name);
                        }
                    }

                    break;
                }
            }
        }
    }

    private void AnalyzeTagNamespace(LuaDocTagNamespaceSyntax namespaceSyntax)
    {
        if (namespaceSyntax is { Namespace: { } name })
        {
            builder.TypeManager.SetNamespace(DocumentId, name.RepresentText);
        }
    }

    private void AnalyzeTagUsing(LuaDocTagUsingSyntax usingSyntax)
    {
        if (usingSyntax is { Using: { } name })
        {
            builder.TypeManager.AddUsingNamespace(DocumentId, name.RepresentText);
        }
    }

    private void AnalyzeSimpleTag(LuaDocTagSyntax tagSyntax)
    {
        AttachToNext(tagSyntax);
    }

    private LuaTypeAttribute GetAttribute(LuaDocTagNamedTypeSyntax tagNamedTypeSyntax)
    {
        var attribute = LuaTypeAttribute.None;
        if (tagNamedTypeSyntax.Attribute?.Attributes is { } attributeList)
        {
            foreach (var nameToken in attributeList)
            {
                if (nameToken is { Text: { } name })
                {
                    switch (name)
                    {
                        case "partial":
                        {
                            attribute |= LuaTypeAttribute.Partial;
                            break;
                        }
                        case "exact":
                        {
                            attribute |= LuaTypeAttribute.Exact;
                            break;
                        }
                        case "global":
                        {
                            attribute |= LuaTypeAttribute.Global;
                            break;
                        }
                        case "key":
                        {
                            attribute |= LuaTypeAttribute.Key;
                            break;
                        }
                    }
                }
            }
        }

        return attribute;
    }

    private void AnalyzeTypeSupers(LuaTypeInfo luaTypeInfo, IEnumerable<LuaDocTypeSyntax> extendList)
    {
        foreach (var extend in extendList)
        {
            if (extend is LuaDocNameTypeSyntax or LuaDocGenericTypeSyntax)
            {
                var type = builder.CreateRef(extend);
                luaTypeInfo.AddSuper(type);
            }
        }
    }

    private void AnalyzeTypeGenericParam(
        LuaTypeInfo luaTypeInfo,
        LuaDocGenericDeclareListSyntax generic
    )
    {
        foreach (var param in generic.Params)
        {
            if (param is { Name: { } name })
            {
                var type = param.Type is not null ? new LuaTypeRef(LuaTypeId.Create(param.Type)) : null;
                var declaration = new LuaSymbol(
                    name.RepresentText,
                    type,
                    new GenericParamInfo(new(param)));
                luaTypeInfo.AddGenericParameter(declaration);
            }
        }
    }

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

    private void AnalyzeTypeFields(LuaTypeInfo luaTypeInfo, LuaDocTagSyntax typeTag)
    {
        foreach (var tagField in typeTag.NextOfType<LuaDocTagFieldSyntax>())
        {
            if (tagField.Field is not null)
            {
                if (tagField.Field is { TypeField: { } typeField, Type: { } type, UniqueId: { } id })
                {
                    var keyType = builder.CreateRef(typeField);
                    var valueType = builder.CreateRef(type);
                    luaTypeInfo.AddOperator(TypeOperatorKind.Index,
                        new IndexOperator(keyType, valueType, id));
                    continue;
                }

                if (AnalyzeDocDetailField(tagField.Field) is { } fieldSymbol)
                {
                    luaTypeInfo.AddDeclaration(fieldSymbol);
                }
            }
        }
    }

    private void AnalyzeTagDocBody(LuaTypeInfo luaTypeInfo, LuaDocBodySyntax docBody)
    {
        foreach (var field in docBody.FieldList)
        {
            if (field is { TypeField: { } typeField, Type: { } type, UniqueId: { } id })
            {
                var keyType = builder.CreateRef(typeField);
                var valueType = builder.CreateRef(type);
                luaTypeInfo.AddOperator(TypeOperatorKind.Index,
                    new IndexOperator(keyType, valueType, id));
                continue;
            }

            if (AnalyzeDocDetailField(field) is { } fieldSymbol)
            {
                luaTypeInfo.AddDeclaration(fieldSymbol);
            }
        }
    }

    private static SymbolVisibility GetVisibility(VisibilityKind visibility)
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
