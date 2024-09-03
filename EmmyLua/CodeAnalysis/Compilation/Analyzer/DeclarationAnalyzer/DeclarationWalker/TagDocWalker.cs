using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeTagClass(LuaDocTagClassSyntax tagClassSyntax)
    {
        if (tagClassSyntax is { Name: { } name })
        {
            var luaClass = new LuaNamedType(declarationContext.DocumentId, name.RepresentText);
            var attribute = GetAttribute(tagClassSyntax);

            var typeInfo = declarationContext.TypeManager.AddTypeDefinition(
                tagClassSyntax,
                name.RepresentText,
                NamedTypeKind.Class,
                attribute);
            if (typeInfo is null)
            {
                declarationContext.AddDiagnostic(new Diagnostic(
                    DiagnosticSeverity.Error,
                    DiagnosticCode.DuplicateType,
                    $"Type '{name.RepresentText}' already exists",
                    name.Range
                ));
                return;
            }

            AnalyzeTypeFields(typeInfo, luaClass, tagClassSyntax);
            AnalyzeTypeOperator(typeInfo, luaClass, tagClassSyntax);

            if (tagClassSyntax is { Body: { } body })
            {
                AnalyzeTagDocBody(typeInfo, luaClass, body);
            }

            // if (tagClassSyntax is { ExtendTypeList: { } extendTypeList })
            // {
            //     AnalyzeTypeSupers(typeInfo, luaClass, extendTypeList);
            // }

            if (tagClassSyntax is { GenericDeclareList: { } genericDeclareList })
            {
                AnalyzeTypeGenericParam(typeInfo, genericDeclareList);
            }
        }

        declarationContext.AttachDoc(tagClassSyntax);
    }

    private void AnalyzeTagAlias(LuaDocTagAliasSyntax tagAliasSyntax)
    {
        if (tagAliasSyntax is { Name: { } name, Type: { } type })
        {
            var attribute = GetAttribute(tagAliasSyntax);
            // var luaAlias = new LuaNamedType(DocumentId, name.RepresentText);
            // var baseTy = searchContext.Infer(type);

            var typeInfo = declarationContext.TypeManager.AddTypeDefinition(
                tagAliasSyntax,
                name.RepresentText,
                NamedTypeKind.Alias,
                attribute);
            if (typeInfo is null)
            {
                declarationContext.AddDiagnostic(new Diagnostic(
                    DiagnosticSeverity.Error,
                    DiagnosticCode.DuplicateType,
                    $"Type '{name.RepresentText}' already exists",
                    name.Range
                ));
            }

            // declarationContext.TypeManager.SetBaseType(luaAlias, baseTy);
        }

        declarationContext.AttachDoc(tagAliasSyntax);
    }

    private void AnalyzeTagEnum(LuaDocTagEnumSyntax tagEnumSyntax)
    {
        if (tagEnumSyntax is { Name: { } name })
        {
            // var luaEnum = new LuaNamedType(DocumentId, name.RepresentText);
            var attribute = GetAttribute(tagEnumSyntax);
            var typeInfo = declarationContext.TypeManager.AddTypeDefinition(
                tagEnumSyntax,
                name.RepresentText,
                NamedTypeKind.Enum,
                attribute);

            if (typeInfo is null)
            {
                declarationContext.AddDiagnostic(new Diagnostic(
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
        }

        declarationContext.AttachDoc(tagEnumSyntax);
    }

    private void AnalyzeTagInterface(LuaDocTagInterfaceSyntax tagInterfaceSyntax)
    {
        if (tagInterfaceSyntax is { Name: { } name })
        {
            var luaInterface = new LuaNamedType(DocumentId, name.RepresentText);
            var attribute = GetAttribute(tagInterfaceSyntax);
            var luaTypeInfo = declarationContext.TypeManager.AddTypeDefinition(
                tagInterfaceSyntax,
                name.RepresentText,
                NamedTypeKind.Interface,
                attribute);

            if (luaTypeInfo is null)
            {
                declarationContext.AddDiagnostic(new Diagnostic(
                    DiagnosticSeverity.Error,
                    DiagnosticCode.DuplicateType,
                    $"Type '{name.RepresentText}' already exists",
                    name.Range
                ));
                return;
            }

            AnalyzeTypeFields(luaTypeInfo,luaInterface, tagInterfaceSyntax);
            AnalyzeTypeOperator(luaTypeInfo,luaInterface, tagInterfaceSyntax);
            if (tagInterfaceSyntax is { Body: { } body })
            {
                AnalyzeTagDocBody(luaTypeInfo, luaInterface, body);
            }

            // if (tagInterfaceSyntax is { ExtendTypeList: { } extendTypeList })
            // {
            //     AnalyzeTypeSupers(luaTypeInfo, luaInterface, extendTypeList);
            // }

            if (tagInterfaceSyntax is { GenericDeclareList: { } genericDeclareList })
            {
                AnalyzeTypeGenericParam(luaTypeInfo, genericDeclareList);
            }
        }

        declarationContext.AttachDoc(tagInterfaceSyntax);
    }

    private void AnalyzeTagType(LuaDocTagTypeSyntax tagTypeSyntax)
    {
        declarationContext.AttachDoc(tagTypeSyntax);
    }

    // private void AnalyzeTypeSupers(LuaTypeInfo luaTypeInfo, LuaNamedType namedType,
    //     IEnumerable<LuaDocTypeSyntax> extendList)
    // {
    //     foreach (var extend in extendList)
    //     {
    //         if (extend is LuaDocTableTypeSyntax { Body: { } body })
    //         {
    //             AnalyzeTagDocBody(luaTypeInfo, namedType, body);
    //         }
    //         else
    //         {
    //             var type = searchContext.Infer(extend);
    //             if (type is LuaNamedType superNamedType)
    //             {
    //                 luaTypeInfo.AddSuper(superNamedType);
    //             }
    //         }
    //     }
    // }

    private void AnalyzeTypeGenericParam(
        LuaTypeInfo luaTypeInfo,
        LuaDocGenericDeclareListSyntax generic
    )
    {
        foreach (var param in generic.Params)
        {
            if (param is { Name: { } name })
            {
                var type = param.Type is not null ? new LuaTypeRef(TypeId.Create(param.Type)) : null;
                var declaration = new LuaSymbol(
                    name.RepresentText,
                    type,
                    new GenericParamInfo(new(param)));
                luaTypeInfo.AddGenericParameter(declaration);
            }
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
            declarationContext.TypeManager.SetNamespace(DocumentId, name.RepresentText);
        }
    }

    private void AnalyzeTagUsing(LuaDocTagUsingSyntax usingSyntax)
    {
        if (usingSyntax is { Using: { } name })
        {
            declarationContext.TypeManager.AddUsingNamespace(DocumentId, name.RepresentText);
        }
    }

    private void AnalyzeTagSource(LuaDocTagSourceSyntax sourceSyntax)
    {
        declarationContext.AttachDoc(sourceSyntax);
    }

    private void AnalyzeSimpleTag(LuaDocTagSyntax tagSyntax)
    {
        declarationContext.AttachDoc(tagSyntax);
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
}
