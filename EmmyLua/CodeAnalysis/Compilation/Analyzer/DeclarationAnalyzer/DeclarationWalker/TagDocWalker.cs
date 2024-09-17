using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
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
            var isTypeCompute = tagClassSyntax.ExtendTypeList
                .Any(typeSyntax => typeSyntax is not LuaDocNameTypeSyntax && typeSyntax is not LuaDocGenericTypeSyntax);

            if (isTypeCompute)
            {
                var typeInfo = builder.TypeManager.AddTypeComputer(tagClassSyntax, name.RepresentText);
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
            else
            {
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
                AttachTypeToNext(luaClass, tagClassSyntax);
            }
        }
    }

    private void AnalyzeTagAlias(LuaDocTagAliasSyntax tagAliasSyntax)
    {
        if (tagAliasSyntax is { Name: { } name, Type: { } type })
        {
            var attribute = GetAttribute(tagAliasSyntax);
            var typeInfo = builder.TypeManager.AddTypeDefinition(
                tagAliasSyntax,
                name.RepresentText,
                NamedTypeKind.Alias,
                attribute);

            typeInfo?.AddBaseType(builder.CreateRef(type));
            if (typeInfo is null)
            {
                builder.AddDiagnostic(new Diagnostic(
                    DiagnosticSeverity.Error,
                    DiagnosticCode.DuplicateType,
                    $"Type '{name.RepresentText}' already exists",
                    name.Range
                ));
            }

            var luaAlias = new LuaNamedType(DocumentId, name.RepresentText);
            AttachTypeToNext(luaAlias, tagAliasSyntax);
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
            var luaTypeInfo = builder.TypeManager.AddTypeDefinition(
                tagInterfaceSyntax,
                name.RepresentText,
                NamedTypeKind.Interface,
                attribute);

            if (luaTypeInfo is null)
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
}
