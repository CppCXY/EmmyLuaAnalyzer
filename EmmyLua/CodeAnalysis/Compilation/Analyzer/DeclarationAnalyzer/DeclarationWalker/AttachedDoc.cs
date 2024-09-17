using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private Dictionary<SyntaxElementId, List<LuaElementPtr<LuaDocTagSyntax>>> _attachedDocs = new();

    private Dictionary<SyntaxElementId, List<LuaType>> _attachedTypes = new();

    private void AttachToNext(LuaDocTagSyntax docTagSyntax)
    {
        var comment = docTagSyntax.Parent;
        if (comment is LuaCommentSyntax)
        {
            if (_attachedDocs.TryGetValue(comment.UniqueId, out var list))
            {
                list.Add(new LuaElementPtr<LuaDocTagSyntax>(docTagSyntax));
            }
            else
            {
                _attachedDocs.Add(comment.UniqueId, [new(docTagSyntax)]);
            }
        }
    }

    private void AttachTypeToNext(LuaType type, LuaDocTagSyntax docTagSyntax)
    {
        var comment = docTagSyntax.Parent;
        if (comment is LuaCommentSyntax)
        {
            if (_attachedTypes.TryGetValue(comment.UniqueId, out var list))
            {
                list.Add(type);
            }
            else
            {
                _attachedTypes.Add(comment.UniqueId, [type]);
            }
        }
    }

    public void FinishAttachedAnalyze()
    {
        foreach (var (elementId, tagSyntaxes) in _attachedDocs)
        {
            if (elementId.ToSyntaxElement(Compilation) is LuaCommentSyntax { Owner: { } attachedElement })
            {
                var docList = new List<LuaDocTagSyntax>();
                foreach (var elementPtr in tagSyntaxes)
                {
                    var docTag = elementPtr.ToNode(builder.Document);
                    if (docTag is not null)
                    {
                        docList.Add(docTag);
                    }

                    GeneralAttachedDoc(attachedElement, docList);
                }
            }
        }
    }

    private void GeneralAttachedDoc(LuaSyntaxElement attachedElement, List<LuaDocTagSyntax> docTagSyntaxes)
    {
        var declaration = FindDeclaration(attachedElement);
        if (declaration is null)
        {
            return;
        }

        foreach (var docTagSyntax in docTagSyntaxes)
        {
            switch (docTagSyntax)
            {
                case LuaDocTagDeprecatedSyntax:
                {
                    declaration.Feature |= SymbolFeature.Deprecated;
                    break;
                }
                case LuaDocTagVisibilitySyntax visibilitySyntax:
                {
                    declaration.Visibility = visibilitySyntax.Visibility switch
                    {
                        VisibilityKind.Public => SymbolVisibility.Public,
                        VisibilityKind.Protected => SymbolVisibility.Protected,
                        VisibilityKind.Private => SymbolVisibility.Private,
                        VisibilityKind.Package => SymbolVisibility.Package,
                        _ => SymbolVisibility.Public
                    };
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
                    declaration.Feature |= SymbolFeature.NoDiscard;
                    break;
                }
                case LuaDocTagAsyncSyntax:
                {
                    declaration.Feature |= SymbolFeature.Async;
                    break;
                }
                case LuaDocTagMappingSyntax mappingSyntax:
                {
                    if (mappingSyntax.Name is { RepresentText: { } name })
                    {
                        declaration.Name = name;
                        builder.ProjectIndex.AddMapping(declaration.UniqueId, name);
                    }

                    break;
                }
                case LuaDocTagSourceSyntax sourceSyntax:
                {
                    if (sourceSyntax.Source is { Value: { } source })
                    {
                        declaration.Feature |= SymbolFeature.Source;
                        builder.ProjectIndex.AddSource(declaration.UniqueId, source);
                    }

                    break;
                }
            }
        }
    }

    private LuaSymbol? FindDeclaration(LuaSyntaxElement element)
    {
        switch (element)
        {
            case LuaLocalStatSyntax localStatSyntax:
            {
                foreach (var localName in localStatSyntax.NameList)
                {
                    if (builder.FindLocalSymbol(localName) is { } luaDeclaration)
                    {
                        return luaDeclaration;
                    }
                }

                break;
            }
            case LuaAssignStatSyntax assignStatSyntax:
            {
                foreach (var assign in assignStatSyntax.VarList)
                {
                    if (builder.FindLocalSymbol(assign) is { } luaDeclaration)
                    {
                        return luaDeclaration;
                    }
                }

                break;
            }
            case LuaTableFieldSyntax tableFieldSyntax:
            {
                if (builder.FindLocalSymbol(tableFieldSyntax) is { } luaDeclaration)
                {
                    return luaDeclaration;
                }

                break;
            }
            case LuaFuncStatSyntax funcStatSyntax:
            {
                switch (funcStatSyntax)
                {
                    case { IsLocal: true, LocalName: { } name }:
                    {
                        if (builder.FindLocalSymbol(name) is { } luaDeclaration)
                        {
                            return luaDeclaration;
                        }

                        break;
                    }
                    case { IsLocal: false, NameExpr: { } nameExpr }:
                    {
                        if (builder.FindLocalSymbol(nameExpr) is { } luaDeclaration)
                        {
                            return luaDeclaration;
                        }

                        break;
                    }
                    case { IsMethod: true, IndexExpr: { } indexExpr }:
                    {
                        if (builder.FindLocalSymbol(indexExpr) is { } luaDeclaration)
                        {
                            return luaDeclaration;
                        }

                        break;
                    }
                }

                break;
            }
        }

        return null;
    }

    private List<LuaDocTagSyntax> FindAttachedDoc(LuaSyntaxElement element)
    {
        while (element is not null && element is not ICommentOwner)
        {
            element = element.Parent;
        }

        if (element is ICommentOwner commentOwner)
        {
            return commentOwner.Comments.SelectMany(it => it.DocList).ToList();
        }

        return [];
    }
}
