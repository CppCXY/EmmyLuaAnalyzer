﻿using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;
using EmmyLua.CodeAnalysis.Type.Types;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;

public class AttachDeclarationAnalyzer(
    DeclarationContext declarationContext,
    SearchContext searchContext)
{
    public void Analyze()
    {
        foreach (var (attachedElement, tagSyntaxes) in declarationContext.GetAttachedDocs())
        {
            AnalyzeGeneralDeclaration(attachedElement, tagSyntaxes);
            AnalyzeMethodDeclaration(attachedElement, tagSyntaxes);
            AnalyzeForRangeDeclaration(attachedElement, tagSyntaxes);
        }
    }

    private void AnalyzeGeneralDeclaration(LuaSyntaxElement attachedElement, List<LuaDocTagSyntax> docTagSyntaxes)
    {
        var declarations = FindDeclarations(attachedElement).ToList();
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
                            DeclarationWalker.DeclarationWalker.GetVisibility(visibilitySyntax.Visibility);
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
                            declarationContext.Db.AddMapping(declaration.UniqueId, name);
                        }

                        break;
                    }
                    case LuaDocTagSourceSyntax sourceSyntax:
                    {
                        if (sourceSyntax.Source is { Value: { } source })
                        {
                            declaration.Feature |= SymbolFeature.Source;
                            declarationContext.Db.AddSource(declaration.UniqueId, source);
                        }

                        break;
                    }
                }
            }
        }

        if (attachedElement is LuaLocalStatSyntax or LuaAssignStatSyntax or LuaTableFieldSyntax)
        {
            // general type define
            var nameTypeDefine = docTagSyntaxes.OfType<LuaDocTagNamedTypeSyntax>().FirstOrDefault();
            if (nameTypeDefine is { Name.RepresentText: { } name } &&
                declarations.FirstOrDefault() is { } firstDeclaration)
            {
                var type = new LuaNamedType(declarationContext.DocumentId, name);
                firstDeclaration.Type = type;
                if (firstDeclaration.IsGlobal)
                {
                    declarationContext.TypeManager.SetGlobalTypeSymbol(firstDeclaration.Name, type);
                }

                return;
            }

            // general cast type
            var nameTypeList = docTagSyntaxes.OfType<LuaDocTagTypeSyntax>().FirstOrDefault();
            if (nameTypeList is { TypeList: { } typeList })
            {
                var luaTypeList = typeList.Select(searchContext.Infer).ToList();
                for (var i = 0; i < luaTypeList.Count; i++)
                {
                    if (declarations.Count > i)
                    {
                        if (declarations[i].IsGlobal && declarations[i].Type is GlobalNameType globalNameType)
                        {
                            declarationContext.TypeManager.SetGlobalBaseType(declarationContext.DocumentId,
                                globalNameType, luaTypeList[i]);
                        }
                        else
                        {
                            declarations[i].Type = luaTypeList[i];
                        }
                    }
                }
            }
        }
    }

    private IEnumerable<LuaSymbol> FindDeclarations(LuaSyntaxElement element)
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

    private void AnalyzeMethodDeclaration(LuaSyntaxElement element, List<LuaDocTagSyntax> docTagSyntaxes)
    {
        var closureExpr = declarationContext.GetElementRelatedClosure(element);
        if (closureExpr is null)
        {
            return;
        }

        var typeInfo = declarationContext.TypeManager.FindTypeInfo(closureExpr.UniqueId);
        if (typeInfo?.BaseType is not LuaMethodType methodType)
        {
            return;
        }

        var genericParams = new List<LuaTypeTemplate>();
        var overloads = new List<LuaSignature>();
        var parameterDict = new Dictionary<string, ParameterInfo>();
        foreach (var docTagSyntax in docTagSyntaxes)
        {
            if (docTagSyntax is LuaDocTagOverloadSyntax overloadSyntax)
            {
                var func = searchContext.Infer(overloadSyntax.TypeFunc);
                if (func is LuaMethodType { MainSignature: { } mainSignature })
                {
                    overloads.Add(mainSignature);
                }
            }
            else if (docTagSyntax is LuaDocTagGenericSyntax genericSyntax)
            {
                foreach (var param in genericSyntax.Params)
                {
                    if (param is { Name: { } name })
                    {
                        genericParams.Add(new LuaTypeTemplate(name.RepresentText,
                            searchContext.Infer(param.Type)));
                    }
                }
            }
            else if (docTagSyntax is LuaDocTagParamSyntax paramSyntax)
            {
                if (paramSyntax.Name is { RepresentText: { } name })
                {
                    var type = searchContext.Infer(paramSyntax.Type);
                    var nullable = paramSyntax.Nullable;
                    parameterDict[name] = new ParameterInfo(nullable, type);
                }
                else if (paramSyntax.VarArgs is not null)
                {
                    var type = searchContext.Infer(paramSyntax.Type);
                    parameterDict["..."] = new ParameterInfo(true, type);
                }
            }
            else if (docTagSyntax is LuaDocTagReturnSyntax returnSyntax)
            {
                var returnTypes = returnSyntax.TypeList.Select(searchContext.Infer).ToList();
                var returnType = returnTypes.Count switch
                {
                    0 => Builtin.Nil,
                    1 => returnTypes[0],
                    _ => new LuaMultiReturnType(returnTypes)
                };
                methodType.MainSignature.ReturnType = returnType;
            }
        }

        if (overloads.Count == 0)
        {
            overloads = null;
        }

        var parameters = methodType.MainSignature.Parameters;
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

        if (genericParams.Count > 0)
        {
            methodType = new LuaGenericMethodType(
                genericParams,
                new LuaSignature(methodType.MainSignature.ReturnType, parameters),
                overloads,
                methodType.ColonDefine);
        }
        else
        {
            methodType = new LuaMethodType(
                new LuaSignature(methodType.MainSignature.ReturnType, parameters),
                overloads,
                methodType.ColonDefine);
        }

        declarationContext.TypeManager.SetBaseType(closureExpr.UniqueId, methodType);
    }

    private void AnalyzeForRangeDeclaration(LuaSyntaxElement element, List<LuaDocTagSyntax> docTagSyntaxes)
    {
        if (element is not LuaForRangeStatSyntax forRangeStatSyntax)
        {
            return;
        }

        var parameterDict = new Dictionary<string, ParameterInfo>();
        foreach (var paramSyntax in docTagSyntaxes.OfType<LuaDocTagParamSyntax>())
        {
            if (paramSyntax.Name is { RepresentText: { } name })
            {
                var type = searchContext.Infer(paramSyntax.Type);
                var nullable = paramSyntax.Nullable;
                parameterDict[name] = new ParameterInfo(nullable, type);
            }
        }

        foreach (var paramDef in forRangeStatSyntax.IteratorNames)
        {
            var declaration = declarationContext.GetAttachedDeclaration(paramDef);
            if (declaration?.Type is LuaElementType elementType &&
                parameterDict.TryGetValue(declaration.Name, out var parameterInfo))
            {
                declarationContext.TypeManager.SetBaseType(elementType.Id, parameterInfo.Type);
            }
        }
    }
}
