using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeDiagnostic(LuaDocTagDiagnosticSyntax diagnosticSyntax)
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

    private void AnalyzeModule(LuaDocTagModuleSyntax moduleSyntax)
    {
        if (moduleSyntax.Module is { Value: { } moduleName })
        {
            Compilation.Workspace.ModuleManager.AddVirtualModule(DocumentId, moduleName);
        }
    }

    private void AnalyzeDeclarationDoc(LuaDeclaration declaration, LuaStatSyntax statSyntax)
    {
        var comment = statSyntax.Comments.FirstOrDefault();
        if (comment?.DocList is { } docList)
        {
            foreach (var tagSyntax in docList)
            {
                switch (tagSyntax)
                {
                    case LuaDocTagDeprecatedSyntax:
                    {
                        declaration.Feature |= DeclarationFeature.Deprecated;
                        break;
                    }
                    case LuaDocTagVisibilitySyntax visibilitySyntax:
                    {
                        declaration.Visibility = GetVisibility(visibilitySyntax.Visibility);
                        break;
                    }
                    case LuaDocTagVersionSyntax versionSyntax:
                    {
                        var requiredVersions = new List<RequiredVersion>();
                        foreach (var version in versionSyntax.Versions)
                        {
                            var action = version.Action;
                            var framework = version.Version?.RepresentText ?? string.Empty;
                            var versionNumber = version.VersionNumber?.Version ?? new VersionNumber(0, 0, 0, 0);
                            requiredVersions.Add(new RequiredVersion(action, framework, versionNumber));
                        }

                        declaration.RequiredVersions = requiredVersions;
                        break;
                    }
                    case LuaDocTagNodiscardSyntax:
                    {
                        declaration.Feature |= DeclarationFeature.NoDiscard;
                        break;
                    }
                    case LuaDocTagAsyncSyntax:
                    {
                        declaration.Feature |= DeclarationFeature.Async;
                        break;
                    }
                }
            }
        }
    }

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

    private static DeclarationVisibility GetVisibility(VisibilityKind visibility)
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
