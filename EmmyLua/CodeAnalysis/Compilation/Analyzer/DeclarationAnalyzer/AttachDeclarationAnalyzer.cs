using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

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
                        declaration.Feature |= DeclarationFeature.Deprecated;
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
                        declaration.Feature |= DeclarationFeature.NoDiscard;
                        break;
                    }
                    case LuaDocTagAsyncSyntax:
                    {
                        declaration.Feature |= DeclarationFeature.Async;
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
                firstDeclaration.Info = firstDeclaration.Info with { DeclarationType = LuaNamedType.Create(name) };
                if (firstDeclaration.IsGlobal)
                {
                    searchContext.Compilation.Db.AddGlobal(declarationContext.DocumentId, firstDeclaration.Name, firstDeclaration, true);
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
                        declarations[i].Info = declarations[i].Info with { DeclarationType = luaTypeList[i] };
                        if (declarations[i].IsGlobal)
                        {
                            searchContext.Compilation.Db.AddGlobal(declarationContext.DocumentId, declarations[i].Name, declarations[i], true);
                        }
                    }
                }

                return;
            }
        }
    }

    private IEnumerable<LuaDeclaration> FindDeclarations(LuaSyntaxElement element)
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

        var idType = declarationContext.Db.QueryTypeFromId(closureExpr.UniqueId);
        if (idType is not LuaMethodType methodType)
        {
            return;
        }

        var genericParams = new List<LuaDeclaration>();
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
                        var declaration = new LuaDeclaration(
                            name.RepresentText,
                            new GenericParamInfo(
                                new(param),
                                searchContext.Infer(param.Type)
                            )
                        );
                        genericParams.Add(declaration);
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
            if (parameter is LuaDeclaration { Name: { } name, Info: ParamInfo {} info } declaration)
            {
                if (parameterDict.TryGetValue(name, out var parameterInfo))
                {
                    declaration.Info = info with
                    {
                        DeclarationType = parameterInfo.Type,
                        Nullable = parameterInfo.Nullable,
                        IsVararg = name == "..."
                    };
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

        declarationContext.Db.UpdateIdRelatedType(closureExpr.UniqueId, methodType);
    }
}
