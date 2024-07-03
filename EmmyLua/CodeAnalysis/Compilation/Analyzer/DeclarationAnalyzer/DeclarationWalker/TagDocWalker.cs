using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeTagClass(LuaDocTagClassSyntax tagClassSyntax)
    {
        if (tagClassSyntax is { Name: { } name })
        {
            var luaClass = new LuaNamedType(name.RepresentText);
            var declaration = new LuaDeclaration(
                name.RepresentText,
                new NamedTypeInfo(
                    new(tagClassSyntax),
                    luaClass,
                    NamedTypeKind.Class
                )
            );

            declarationContext.Db.AddTypeDefinition(DocumentId, name.RepresentText, declaration);

            AnalyzeTypeFields(luaClass, tagClassSyntax);
            AnalyzeTypeOperator(luaClass, tagClassSyntax);

            if (tagClassSyntax is { Body: { } body })
            {
                AnalyzeDocBody(luaClass, body);
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
            var luaAlias = new LuaNamedType(name.RepresentText);
            var baseTy = searchContext.Infer(type);
            var declaration = new LuaDeclaration(
                name.RepresentText,
                new NamedTypeInfo(
                    new(tagAliasSyntax),
                    luaAlias,
                    NamedTypeKind.Alias
                ));
            declarationContext.Db.AddAlias(DocumentId, name.RepresentText, baseTy, declaration);
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
            var luaEnum = new LuaNamedType(name.RepresentText);
            var declaration = new LuaDeclaration(
                name.RepresentText,
                new NamedTypeInfo(
                    new(tagEnumSyntax),
                    luaEnum,
                    NamedTypeKind.Enum
                ));

            declarationContext.Db.AddEnum(DocumentId, name.RepresentText, baseType, declaration);
            foreach (var field in tagEnumSyntax.FieldList)
            {
                if (field is { Name: { } fieldName })
                {
                    var fieldDeclaration = new LuaDeclaration(
                        fieldName.RepresentText,
                        new EnumFieldInfo(
                            new(field),
                            baseType
                        ));
                    declarationContext.Db.AddMember(DocumentId, luaEnum, fieldDeclaration);
                }
            }
        }

        declarationContext.AttachDoc(tagEnumSyntax);
    }

    private void AnalyzeTagInterface(LuaDocTagInterfaceSyntax tagInterfaceSyntax)
    {
        if (tagInterfaceSyntax is { Name: { } name })
        {
            var luaInterface = new LuaNamedType(name.RepresentText);
            var declaration = new LuaDeclaration(
                name.RepresentText,
                new NamedTypeInfo(
                    new(tagInterfaceSyntax),
                    luaInterface,
                    NamedTypeKind.Interface
                ));

            declarationContext.Db.AddTypeDefinition(DocumentId, name.RepresentText, declaration);
            AnalyzeTypeFields(luaInterface, tagInterfaceSyntax);
            AnalyzeTypeOperator(luaInterface, tagInterfaceSyntax);
            if (tagInterfaceSyntax is { Body: { } body })
            {
                AnalyzeDocBody(luaInterface, body);
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
        foreach (var extend in extendList)
        {
            var type = searchContext.Infer(extend);
            declarationContext.Db.AddSuper(DocumentId, namedType.Name, type);
        }
    }

    private void AnalyzeTypeGenericParam(LuaDocGenericDeclareListSyntax generic,
        LuaNamedType namedType)
    {
        foreach (var param in generic.Params)
        {
            if (param is { Name: { } name })
            {
                var type = param.Type is not null ? searchContext.Infer(param.Type) : null;
                var declaration = new LuaDeclaration(
                    name.RepresentText,
                    new GenericParamInfo(
                        new(param),
                        type
                    ));
                declarationContext.Db.AddGenericParam(DocumentId, namedType.Name, declaration);
            }
        }
    }

    private void AnalyzeTagModule(LuaDocTagModuleSyntax moduleSyntax)
    {
        if (moduleSyntax.Module is { Value: { } moduleName })
        {
            Compilation.Workspace.ModuleManager.AddVirtualModule(DocumentId, moduleName);
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

    private void AnalyzeSimpleTag(LuaDocTagSyntax tagSyntax)
    {
        declarationContext.AttachDoc(tagSyntax);
    }
}
