using EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Index;
using EmmyLua.CodeAnalysis.Compilation.Signature;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.AttachDocAnalyzer;

public class AttachDocAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "AttachDeclaration")
{
    private ProjectIndex ProjectIndex => Compilation.ProjectIndex;

    private GlobalIndex GlobalIndex => Compilation.GlobalIndex;

    private LuaTypeManager TypeManager => Compilation.TypeManager;

    public override void Analyze(AnalyzeContext analyzeContext)
    {
        foreach (var declarationContext in analyzeContext.DeclarationContexts)
        {
            foreach (var (attachedElement, tagSyntaxes) in declarationContext.GetAttachedDocs())
            {
                AnalyzeGeneralDeclaration(attachedElement, tagSyntaxes, declarationContext);
                AnalyzeMethodDeclaration(attachedElement, tagSyntaxes, declarationContext);
                AnalyzeForRangeDeclaration(attachedElement, tagSyntaxes, declarationContext);
            }
        }
    }

    private void AnalyzeGeneralDeclaration(LuaSyntaxElement attachedElement, List<LuaDocTagSyntax> docTagSyntaxes,
        DeclarationContext declarationContext)
    {
        var declarations = FindDeclarations(attachedElement, declarationContext).ToList();
        foreach (var declaration in declarations)
        {
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
                        declaration.Visibility =
                            DeclarationAnalyzer.DeclarationWalker.DeclarationWalker.GetVisibility(visibilitySyntax
                                .Visibility);
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
                            ProjectIndex.AddMapping(declaration.UniqueId, name);
                        }

                        break;
                    }
                    case LuaDocTagSourceSyntax sourceSyntax:
                    {
                        if (sourceSyntax.Source is { Value: { } source })
                        {
                            declaration.Feature |= SymbolFeature.Source;
                            ProjectIndex.AddSource(declaration.UniqueId, source);
                        }

                        break;
                    }
                }
            }
        }

        if (attachedElement is LuaLocalStatSyntax or LuaAssignStatSyntax or LuaTableFieldSyntax)
        {
            // general type define
            var nameTypeDefine = docTagSyntaxes.OfType<LuaDocTagNamedTypeSyntax>().LastOrDefault();
            if (nameTypeDefine is { Name.RepresentText: { } name } &&
                declarations.FirstOrDefault() is { } firstDeclaration)
            {
                var type = new LuaNamedType(attachedElement.DocumentId, name);
                firstDeclaration.Type = type;
                if (firstDeclaration.IsGlobal)
                {
                    GlobalIndex.AddDefinedDocumentId(firstDeclaration.Name,
                        firstDeclaration.DocumentId);
                }

                return;
            }

            // general cast type
            var nameTypeList = docTagSyntaxes.OfType<LuaDocTagTypeSyntax>().FirstOrDefault();
            if (nameTypeList is { TypeList: { } typeList })
            {
                var luaTypeList = typeList.Select(it => new LuaTypeRef(TypeId.Create(it))).ToList();
                for (var i = 0; i < luaTypeList.Count; i++)
                {
                    if (declarations.Count > i)
                    {
                        declarations[i].Type = luaTypeList[i];

                        if (declarations[i].IsGlobal)
                        {
                            GlobalIndex.AddDefinedDocumentId(declarations[i].Name,
                                declarations[i].DocumentId);
                        }
                    }
                }
            }
        }
    }

    private IEnumerable<LuaSymbol> FindDeclarations(LuaSyntaxElement element, DeclarationContext declarationContext)
    {
        switch (element)
        {
            case LuaLocalStatSyntax localStatSyntax:
            {
                foreach (var localName in localStatSyntax.NameList)
                {
                    if (declarationContext.GetAttachedDeclaration(localName) is { } luaDeclaration)
                    {
                        yield return luaDeclaration;
                    }
                }

                break;
            }
            case LuaAssignStatSyntax assignStatSyntax:
            {
                foreach (var assign in assignStatSyntax.VarList)
                {
                    if (declarationContext.GetAttachedDeclaration(assign) is { } luaDeclaration)
                    {
                        yield return luaDeclaration;
                    }
                }

                break;
            }
            case LuaTableFieldSyntax tableFieldSyntax:
            {
                if (declarationContext.GetAttachedDeclaration(tableFieldSyntax) is { } luaDeclaration)
                {
                    yield return luaDeclaration;
                }

                break;
            }
            case LuaFuncStatSyntax funcStatSyntax:
            {
                switch (funcStatSyntax)
                {
                    case { IsLocal: true, LocalName: { } name }:
                    {
                        if (declarationContext.GetAttachedDeclaration(name) is { } luaDeclaration)
                        {
                            yield return luaDeclaration;
                        }

                        break;
                    }
                    case { IsLocal: false, NameExpr: { } nameExpr }:
                    {
                        if (declarationContext.GetAttachedDeclaration(nameExpr) is { } luaDeclaration)
                        {
                            yield return luaDeclaration;
                        }

                        break;
                    }
                    case { IsMethod: true, IndexExpr: { } indexExpr }:
                    {
                        if (declarationContext.GetAttachedDeclaration(indexExpr) is { } luaDeclaration)
                        {
                            yield return luaDeclaration;
                        }

                        break;
                    }
                }

                break;
            }
            default:
            {
                yield break;
            }
        }
    }

    private record struct ParameterInfo(bool Nullable, LuaType Type);

    private void AnalyzeMethodDeclaration(LuaSyntaxElement element, List<LuaDocTagSyntax> docTagSyntaxes,
        DeclarationContext declarationContext)
    {
        var closureExpr = declarationContext.GetElementRelatedClosure(element);
        if (closureExpr is null)
        {
            return;
        }

        var id = LuaSignatureId.Create(closureExpr);
        var luaSignature = declarationContext.SignatureManager.GetSignature(id);
        if (luaSignature is null)
        {
            return;
        }

        var parameterDict = new Dictionary<string, ParameterInfo>();
        foreach (var docTagSyntax in docTagSyntaxes)
        {
            if (docTagSyntax is LuaDocTagOverloadSyntax overloadSyntax)
            {
                // var func = searchContext.Infer(overloadSyntax.TypeFunc);
                // if (func is LuaMethodType { MainSignature: { } mainSignature })
                // {
                //     overloads.Add(mainSignature);
                // }
            }
            else if (docTagSyntax is LuaDocTagParamSyntax paramSyntax)
            {
                if (paramSyntax is { Name.RepresentText: { } name, Type: { } paramType })
                {
                    var type = new LuaTypeRef(TypeId.Create(paramType));
                    var nullable = paramSyntax.Nullable;
                    parameterDict[name] = new ParameterInfo(nullable, type);
                }
                else if (paramSyntax is { VarArgs: { } _, Type: { } paramType2 })
                {
                    var type = new LuaTypeRef(TypeId.Create(paramType2));
                    parameterDict["..."] = new ParameterInfo(true, type);
                }
            }
            else if (docTagSyntax is LuaDocTagReturnSyntax returnSyntax)
            {
                var returnTypes = returnSyntax.TypeList
                    .Select(it => new LuaTypeRef(TypeId.Create(it))).Cast<LuaType>()
                    .ToList();
                var returnType = returnTypes.Count switch
                {
                    0 => Builtin.Nil,
                    1 => returnTypes[0],
                    _ => new LuaMultiReturnType(returnTypes)
                };
                luaSignature.ReturnType = returnType;
            }
        }

        var parameters = luaSignature.Parameters;
        foreach (var parameter in parameters)
        {
            if (parameter is { Name: { } name, Info: ParamInfo { } info } declaration)
            {
                if (parameterDict.TryGetValue(name, out var parameterInfo))
                {
                    declaration.Info = info with
                    {
                        Nullable = parameterInfo.Nullable,
                        IsVararg = name == "..."
                    };

                    declaration.Type = parameterInfo.Type;
                }
            }
        }
    }

    private void AnalyzeForRangeDeclaration(LuaSyntaxElement element, List<LuaDocTagSyntax> docTagSyntaxes,
        DeclarationContext declarationContext)
    {
        if (element is not LuaForRangeStatSyntax forRangeStatSyntax)
        {
            return;
        }

        var parameterDict = new Dictionary<string, ParameterInfo>();
        foreach (var paramSyntax in docTagSyntaxes.OfType<LuaDocTagParamSyntax>())
        {
            if (paramSyntax is { Name.RepresentText: { } name, Type: { } paramType })
            {
                var type = new LuaTypeRef(TypeId.Create(paramType));
                var nullable = paramSyntax.Nullable;
                parameterDict[name] = new ParameterInfo(nullable, type);
            }
        }

        foreach (var paramDef in forRangeStatSyntax.IteratorNames)
        {
            var declaration = declarationContext.GetAttachedDeclaration(paramDef);
            if (declaration is null)
            {
                continue;
            }

            if (parameterDict.TryGetValue(declaration.Name, out var parameterInfo))
            {
                declaration.Type = parameterInfo.Type;
            }
        }
    }
}
