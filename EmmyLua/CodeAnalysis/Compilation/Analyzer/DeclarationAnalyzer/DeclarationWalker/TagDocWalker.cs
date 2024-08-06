using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeTagClass(LuaDocTagClassSyntax tagClassSyntax)
    {
        if (tagClassSyntax is { Name: { } name })
        {
            var luaClass = new LuaNamedType(declarationContext.DocumentId, name.RepresentText);
            var attribute = GetAttribute(tagClassSyntax);

            if (!declarationContext.TypeManager.AddTypeDefinition(tagClassSyntax, name.RepresentText,
                    NamedTypeKind.Class, attribute))
            {
                declarationContext.AddDiagnostic(new Diagnostic(
                    DiagnosticSeverity.Error,
                    DiagnosticCode.DuplicateType,
                    $"Type '{name.RepresentText}' already exists",
                    name.Range
                ));
            }

            AnalyzeTypeFields(luaClass, tagClassSyntax);
            AnalyzeTypeOperator(luaClass, tagClassSyntax);

            if (tagClassSyntax is { Body: { } body })
            {
                AnalyzeTagDocBody(luaClass, body);
            }

            if (tagClassSyntax is { ExtendTypeList: { } extendTypeList })
            {
                AnalyzeTypeSupers(extendTypeList, luaClass);
            }

            if (tagClassSyntax is { GenericDeclareList: { } genericDeclareList })
            {
                AnalyzeTypeGenericParam(genericDeclareList, luaClass);
            }
        }

        declarationContext.AttachDoc(tagClassSyntax);
    }

    private void AnalyzeTagAlias(LuaDocTagAliasSyntax tagAliasSyntax)
    {
        if (tagAliasSyntax is { Name: { } name, Type: { } type })
        {
            var attribute = GetAttribute(tagAliasSyntax);
            var luaAlias = new LuaNamedType(DocumentId, name.RepresentText);
            var baseTy = searchContext.Infer(type);

            if (!declarationContext.TypeManager.AddTypeDefinition(tagAliasSyntax, name.RepresentText,
                    NamedTypeKind.Alias, attribute))
            {
                declarationContext.AddDiagnostic(new Diagnostic(
                    DiagnosticSeverity.Error,
                    DiagnosticCode.DuplicateType,
                    $"Type '{name.RepresentText}' already exists",
                    name.Range
                ));
            }

            declarationContext.TypeManager.SetBaseType(luaAlias, baseTy);
        }

        declarationContext.AttachDoc(tagAliasSyntax);
    }

    private void AnalyzeTagEnum(LuaDocTagEnumSyntax tagEnumSyntax)
    {
        if (tagEnumSyntax is { Name: { } name })
        {
            var baseType = tagEnumSyntax.BaseType is { } type
                ? searchContext.Infer(type)
                : Builtin.Integer;
            var luaEnum = new LuaNamedType(DocumentId, name.RepresentText);
            var attribute = GetAttribute(tagEnumSyntax);

            if (!declarationContext.TypeManager.AddTypeDefinition(tagEnumSyntax, name.RepresentText,
                    NamedTypeKind.Enum, attribute))
            {
                declarationContext.AddDiagnostic(new Diagnostic(
                    DiagnosticSeverity.Error,
                    DiagnosticCode.DuplicateType,
                    $"Type '{name.RepresentText}' already exists",
                    name.Range
                ));
            }

            declarationContext.TypeManager.SetBaseType(luaEnum, baseType);
            var enumFields = new List<LuaSymbol>();
            foreach (var field in tagEnumSyntax.FieldList)
            {
                if (field is { Name: { } fieldName })
                {
                    var fieldDeclaration = new LuaSymbol(
                        fieldName.RepresentText,
                        baseType,
                        new EnumFieldInfo(new(field)));
                    enumFields.Add(fieldDeclaration);
                }
            }

            if (enumFields.Count > 0)
            {
                declarationContext.TypeManager.AddMemberDeclarations(luaEnum, enumFields);
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
            if (!declarationContext.TypeManager.AddTypeDefinition(tagInterfaceSyntax, name.RepresentText,
                    NamedTypeKind.Interface, attribute))
            {
                declarationContext.AddDiagnostic(new Diagnostic(
                    DiagnosticSeverity.Error,
                    DiagnosticCode.DuplicateType,
                    $"Type '{name.RepresentText}' already exists",
                    name.Range
                ));
            }

            AnalyzeTypeFields(luaInterface, tagInterfaceSyntax);
            AnalyzeTypeOperator(luaInterface, tagInterfaceSyntax);
            if (tagInterfaceSyntax is { Body: { } body })
            {
                AnalyzeTagDocBody(luaInterface, body);
            }

            if (tagInterfaceSyntax is { ExtendTypeList: { } extendTypeList })
            {
                AnalyzeTypeSupers(extendTypeList, luaInterface);
            }

            if (tagInterfaceSyntax is { GenericDeclareList: { } genericDeclareList })
            {
                AnalyzeTypeGenericParam(genericDeclareList, luaInterface);
            }
        }

        declarationContext.AttachDoc(tagInterfaceSyntax);
    }

    private void AnalyzeTagType(LuaDocTagTypeSyntax tagTypeSyntax)
    {
        declarationContext.AttachDoc(tagTypeSyntax);
    }

    private void AnalyzeTypeSupers(IEnumerable<LuaDocTypeSyntax> extendList, LuaNamedType namedType)
    {
        var supers = new List<LuaNamedType>();
        foreach (var extend in extendList)
        {
            if (extend is LuaDocTableTypeSyntax { Body: { } body })
            {
                AnalyzeTagDocBody(namedType, body);
            }
            else
            {
                var type = searchContext.Infer(extend);
                if (type is LuaNamedType superNamedType)
                {
                    supers.Add(superNamedType);
                }
            }
        }

        if (supers.Count > 0)
        {
            declarationContext.TypeManager.AddSupers(namedType, supers);
        }
    }

    private void AnalyzeTypeGenericParam(LuaDocGenericDeclareListSyntax generic,
        LuaNamedType namedType)
    {
        var genericParams = new List<LuaSymbol>();
        foreach (var param in generic.Params)
        {
            if (param is { Name: { } name })
            {
                var type = param.Type is not null ? searchContext.Infer(param.Type) : null;
                var declaration = new LuaSymbol(
                    name.RepresentText,
                    type,
                    new GenericParamInfo(new(param)));
                genericParams.Add(declaration);
            }
        }

        if (genericParams.Count > 0)
        {
            declarationContext.TypeManager.AddGenericParams(namedType, genericParams);
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
