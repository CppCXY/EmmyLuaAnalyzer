using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private LuaType? FindFirstLocalOrAssignType(LuaStatSyntax stat)
    {
        foreach (var comment in stat.Comments)
        {
            var tagNameTypeSyntax = comment.DocList.OfType<LuaDocTagNamedTypeSyntax>().FirstOrDefault();
            if (tagNameTypeSyntax is { Name.RepresentText: { } name })
            {
                return new LuaNamedType(name);
            }
        }

        return null;
    }


    private List<LuaType> FindLocalOrAssignTypes(LuaStatSyntax stat) =>
    (
        from comment in stat.Comments
        from tagType in comment.DocList.OfType<LuaDocTagTypeSyntax>()
        from type in tagType.TypeList
        select searchContext.Infer(type)
    ).ToList();


    private void AnalyzeLocalStatDeclaration(LuaLocalStatSyntax localStatSyntax)
    {
        var types = FindLocalOrAssignTypes(localStatSyntax);
        var nameList = localStatSyntax.NameList.ToList();
        var exprList = localStatSyntax.ExprList.ToList();
        LuaExprSyntax? lastValidExpr = null;
        var count = nameList.Count;
        var retId = 0;
        for (var i = 0; i < count; i++)
        {
            var localName = nameList[i];
            var luaType = types.ElementAtOrDefault(i);
            var expr = exprList.ElementAtOrDefault(i);
            if (expr is not null)
            {
                lastValidExpr = expr;
                retId = 0;
            }
            else
            {
                retId++;
            }

            LuaExprRef? relatedExpr = null;
            if (lastValidExpr is not null)
            {
                relatedExpr = new LuaExprRef(lastValidExpr, retId);
            }

            if (localName is { Name: { } name })
            {
                var declaration = new LuaDeclaration(
                    name.RepresentText,
                    new LocalInfo(
                        new(localName),
                        luaType ?? new LuaVariableRefType(localName.UniqueId),
                        localName.Attribute?.IsConst ?? false,
                        localName.Attribute?.IsClose ?? false
                    ),
                    DeclarationFeature.Local
                );
                declarationContext.AddReference(ReferenceKind.Definition, declaration, localName);
                declarationContext.AddDeclaration(localName.Position, declaration);
                var unResolveDeclaration =
                    new UnResolvedDeclaration(declaration, relatedExpr, ResolveState.UnResolvedType);
                declarationContext.AddUnResolved(unResolveDeclaration);
                if (i == 0)
                {
                    var definedType = FindFirstLocalOrAssignType(localStatSyntax);
                    if (definedType is not null && declaration.Info is LocalInfo info)
                    {
                        declaration.Info = info with
                        {
                            DeclarationType = definedType,
                        };
                    }
                }
            }
        }
    }

    private Dictionary<string, (LuaType, bool /* nullable*/)> FindParamDict(LuaSyntaxElement element)
    {
        var stat = element.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault();
        if (stat is null)
        {
            return [];
        }

        var docList = stat.Comments.FirstOrDefault()?.DocList;
        if (docList is null)
        {
            return [];
        }

        var dic = new Dictionary<string, (LuaType, bool /* nullable*/)>();

        foreach (var tagParamSyntax in docList.OfType<LuaDocTagParamSyntax>())
        {
            if (tagParamSyntax is { Name: { } name, Type: { } type, Nullable: { } nullable })
            {
                var ty = searchContext.Infer(type);
                if (nullable)
                {
                    ty = ty.Union(Builtin.Nil);
                }

                dic.TryAdd(name.RepresentText, (ty, nullable));
            }
            else if (tagParamSyntax is { VarArgs: { } _, Type: { } type2, Nullable: { } nullable2 })
            {
                var ty = searchContext.Infer(type2);
                dic.TryAdd("...", (ty, nullable2));
            }
        }

        return dic;
    }

    private List<LuaDeclaration> AnalyzeParamListDeclaration(LuaParamListSyntax paramListSyntax)
    {
        var parameters = new List<LuaDeclaration>();
        var paramDict = FindParamDict(paramListSyntax);
        foreach (var param in paramListSyntax.Params)
        {
            if (param.Name is { } name)
            {
                LuaType? luaType = null;
                var nullable = false;
                if (paramDict.TryGetValue(name.RepresentText, out var paramInfo))
                {
                    luaType = paramInfo.Item1;
                    nullable = paramInfo.Item2;
                }

                var declaration = new LuaDeclaration(
                    name.RepresentText,
                    new ParamInfo(
                        new(param),
                        luaType,
                        false,
                        nullable
                    ),
                    DeclarationFeature.Local
                );
                declarationContext.AddReference(ReferenceKind.Definition, declaration, param);
                parameters.Add(declaration);
                declarationContext.AddDeclaration(param.Position, declaration);
            }
            else if (param.IsVarArgs)
            {
                LuaType? luaType = null;
                var nullable = false;
                if (paramDict.TryGetValue("...", out var paramInfo))
                {
                    luaType = paramInfo.Item1;
                    nullable = paramInfo.Item2;
                }

                var declaration = new LuaDeclaration(
                    "...",
                    new ParamInfo(
                        new(param),
                        luaType ?? new LuaVariableRefType(param.UniqueId),
                        true,
                        nullable
                    ),
                    DeclarationFeature.Local
                );

                parameters.Add(declaration);
                declarationContext.AddDeclaration(param.Position, declaration);
            }
        }

        return parameters;
    }

    private void AnalyzeForRangeStatDeclaration(LuaForRangeStatSyntax forRangeStatSyntax)
    {
        var dic = FindParamDict(forRangeStatSyntax);
        var parameters = new List<LuaDeclaration>();
        foreach (var param in forRangeStatSyntax.IteratorNames)
        {
            if (param.Name is { } name)
            {
                var declaration = new LuaDeclaration(
                    name.RepresentText,
                    new ParamInfo(
                        new(param),
                        new LuaVariableRefType(param.UniqueId),
                        false
                    ),
                    DeclarationFeature.Local);
                if (dic.TryGetValue(name.RepresentText, out var paramInfo))
                {
                    declaration.Info = declaration.Info with { DeclarationType = paramInfo.Item1 };
                }

                declarationContext.AddReference(ReferenceKind.Definition, declaration, param);
                declarationContext.AddDeclaration(param.Position, declaration);
                parameters.Add(declaration);
            }
        }

        var unResolved = new UnResolvedForRangeParameter(parameters, forRangeStatSyntax.ExprList.ToList());
        declarationContext.AddUnResolved(unResolved);
    }

    private void AnalyzeForStatDeclaration(LuaForStatSyntax forStatSyntax)
    {
        if (forStatSyntax is { IteratorName.Name: { } name })
        {
            var declaration = new LuaDeclaration(
                name.RepresentText,
                new ParamInfo(
                    new(forStatSyntax.IteratorName),
                    Builtin.Integer,
                    false
                ),
                DeclarationFeature.Local);
            declarationContext.AddReference(ReferenceKind.Definition, declaration, forStatSyntax.IteratorName);
            declarationContext.AddDeclaration(forStatSyntax.IteratorName.Position, declaration);
        }
    }

 private void AnalyzeAssignStatDeclaration(LuaAssignStatSyntax luaAssignStat)
    {
        var types = FindLocalOrAssignTypes(luaAssignStat);
        var varList = luaAssignStat.VarList.ToList();
        var exprList = luaAssignStat.ExprList.ToList();
        LuaExprSyntax? lastValidExpr = null;
        var retId = 0;
        var count = varList.Count;
        for (var i = 0; i < count; i++)
        {
            var varExpr = varList[i];
            var luaType = types.ElementAtOrDefault(i);
            var expr = exprList.ElementAtOrDefault(i);
            if (expr is not null)
            {
                lastValidExpr = expr;
                retId = 0;
            }
            else
            {
                retId++;
            }

            LuaExprRef? relatedExpr = null;
            if (lastValidExpr is not null)
            {
                relatedExpr = new LuaExprRef(lastValidExpr, retId);
            }

            switch (varExpr)
            {
                case LuaNameExprSyntax nameExpr:
                {
                    if (nameExpr.Name is { } name)
                    {
                        var prevDeclaration = declarationContext.FindLocalDeclaration(nameExpr);
                        if (prevDeclaration is null)
                        {
                            var declaration = new LuaDeclaration(
                                name.RepresentText,
                                new GlobalInfo(
                                    new(nameExpr),
                                    luaType ?? new GlobalNameType(name.RepresentText)
                                ),
                                DeclarationFeature.Global
                            );
                            declarationContext.AddReference(ReferenceKind.Definition, declaration, nameExpr);
                            AnalyzeDeclarationDoc(declaration, luaAssignStat);

                            var unResolveDeclaration =
                                new UnResolvedDeclaration(declaration, relatedExpr, ResolveState.UnResolvedType);
                            declarationContext.AddUnResolved(unResolveDeclaration);
                            var isTypeDeclaration = false;
                            if (i == 0)
                            {
                                var definedType = FindFirstLocalOrAssignType(luaAssignStat);
                                if (definedType is not null && declaration.Info is GlobalInfo info)
                                {
                                    declaration.Info = info with
                                    {
                                        DeclarationType = definedType,
                                        TypeDecl = true
                                    };
                                    isTypeDeclaration = true;
                                }
                            }

                            declarationContext.Db.AddGlobal(DocumentId, isTypeDeclaration,
                                name.RepresentText, declaration);
                            declarationContext.AddDeclaration(nameExpr.Position, declaration);
                        }
                        else
                        {
                            declarationContext.AddReference(ReferenceKind.Write, prevDeclaration, nameExpr);
                        }
                    }

                    break;
                }
                case LuaIndexExprSyntax indexExpr:
                {
                    if (indexExpr.Name is { } name)
                    {
                        var valueExprPtr = relatedExpr?.RetId == 0
                            ? new(relatedExpr.Expr)
                            : LuaElementPtr<LuaExprSyntax>.Empty;
                        var declaration = new LuaDeclaration(
                            name,
                            new IndexInfo(
                                new(indexExpr),
                                valueExprPtr,
                                luaType
                            )
                        );
                        AnalyzeDeclarationDoc(declaration, luaAssignStat);
                        var unResolveDeclaration = new UnResolvedDeclaration(declaration, relatedExpr,
                            ResolveState.UnResolvedType | ResolveState.UnResolvedIndex);
                        declarationContext.AddUnResolved(unResolveDeclaration);
                        if (i == 0)
                        {
                            var declarationType = FindFirstLocalOrAssignType(luaAssignStat);
                            if (declarationType is not null && declaration.Info is IndexInfo indexInfo)
                            {
                                declaration.Info = indexInfo with
                                {
                                    DeclarationType = declarationType,
                                };
                            }
                        }
                    }
                    else if (indexExpr is { IndexKeyExpr: { } indexKeyExpr })
                    {
                    }

                    break;
                }
            }
        }
    }

}
