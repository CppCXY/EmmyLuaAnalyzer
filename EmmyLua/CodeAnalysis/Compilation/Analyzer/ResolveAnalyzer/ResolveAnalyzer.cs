using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;

public class ResolveAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Resolve")
{
    private SearchContext Context { get; set; } = null!;

    public override void Analyze(AnalyzeContext analyzeContext)
    {
        Context = new(Compilation, new SearchContextFeatures()
        {
            Cache = true,
            CacheUnknown = false,
        });

        var resolveDependencyGraph = new ResolveDependencyGraph(Context, analyzeContext);
        resolveDependencyGraph.OnResolved += (unResolved, state) =>
        {
            switch (state)
            {
                case ResolveState.UnResolvedType:
                {
                    ResolveType(unResolved);
                    break;
                }
                case ResolveState.UnResolvedIndex:
                {
                    ResolveIndex(unResolved);
                    break;
                }
                case ResolveState.UnResolveReturn:
                {
                    ResolveReturn(unResolved, analyzeContext);
                    break;
                }
                case ResolveState.UnResolvedParameters:
                {
                    ResolveParameters(unResolved);
                    break;
                }
            }
        };

        resolveDependencyGraph.OnForceTypeResolved += (unResolved, state) =>
        {
            switch (state)
            {
                case ResolveState.UnResolvedType:
                {
                    FinalResolveType(unResolved);
                    break;
                }
            }
        };

        resolveDependencyGraph.Resolve(analyzeContext.UnResolves);
        Context = null!;
    }

    private void ResolveType(UnResolved unResolved)
    {
        if (unResolved is UnResolvedSymbol unResolvedSymbol)
        {
            var exprRef = unResolvedSymbol.ExprRef;
            if (exprRef is not null)
            {
                var exprType = Context.Infer(exprRef.Expr);
                if (!exprType.IsSameType(Builtin.Unknown, Context))
                {
                    MergeType(unResolvedSymbol, exprRef.Expr, exprType, exprRef.RetId);
                }
            }
            else if (unResolvedSymbol.LuaSymbol.Type is null)
            {
                unResolvedSymbol.LuaSymbol.Type = Builtin.Nil;
            }
        }
        else if (unResolved is UnResolvedForRangeParameter unResolvedForRangeParameter)
        {
            var exprList = unResolvedForRangeParameter.ExprList;
            switch (exprList.Count)
            {
                case 0:
                {
                    return;
                }
                // ipairs and pairs
                case 1:
                {
                    var iterExpr = exprList.First();
                    var iterType = Context.Infer(iterExpr);
                    if (iterType is LuaMethodType methodType)
                    {
                        var returnType = methodType.MainSignature.ReturnType;
                        if (returnType is LuaMultiReturnType multiReturnType)
                        {
                            for (var i = 0; i < unResolvedForRangeParameter.Parameters.Count; i++)
                            {
                                var parameter = unResolvedForRangeParameter.Parameters[i];
                                if (parameter.Type is LuaElementType elementType)
                                {
                                    if (Compilation.TypeManager.GetBaseType(elementType.Id) is null)
                                    {
                                        Compilation.TypeManager.SetBaseType(elementType.Id,
                                            multiReturnType.GetElementType(i));
                                    }
                                }
                            }
                        }
                        else if (unResolvedForRangeParameter.Parameters.FirstOrDefault() is
                                 { } firstDeclaration)
                        {
                            if (firstDeclaration.Type is LuaElementType elementType)
                            {
                                if (Compilation.TypeManager.GetBaseType(elementType.Id) is null)
                                {
                                    Compilation.TypeManager.SetBaseType(elementType.Id, returnType);
                                }
                            }
                        }
                    }

                    return;
                }
            }
        }
    }

    private void FinalResolveType(UnResolved unResolved)
    {
        if (unResolved is UnResolvedSymbol unResolvedDeclaration)
        {
            var declaration = unResolvedDeclaration.LuaSymbol;
            declaration.Type ??= new LuaElementType(declaration.UniqueId);
        }
    }

    private void ResolveIndex(UnResolved unResolved)
    {
        if (unResolved is UnResolvedSymbol unResolvedDeclaration)
        {
            var declaration = unResolvedDeclaration.LuaSymbol;
            if (declaration.Info.Ptr.ToNode(Context) is LuaIndexExprSyntax { PrefixExpr: { } prefixExpr })
            {
                var ty = Context.Infer(prefixExpr);
                if (ty is LuaNamedType namedType)
                {
                    Compilation.TypeManager.AddMemberImplementation(namedType, declaration);
                }
                else if (ty is LuaElementType elementType)
                {
                    Compilation.TypeManager.AddElementMember(elementType.Id, declaration);
                }
                else if (ty is GlobalNameType globalNameType)
                {
                    Compilation.TypeManager.AddGlobalMember(globalNameType.Name, declaration);
                }

                // Context.ClearMemberCache(ty);
            }
        }
    }

    private void ResolveReturn(UnResolved unResolved, AnalyzeContext analyzeContext)
    {
        if (unResolved is UnResolvedMethod unResolvedMethod)
        {
            var id = unResolvedMethod.Id;
            var typeInfo = Compilation.TypeManager.FindTypeInfo(id);
            if (typeInfo?.BaseType is LuaMethodType methodType)
            {
                if (methodType.MainSignature.ReturnType.IsSameType(Builtin.Unknown, Context))
                {
                    var block = unResolvedMethod.Block;
                    var returnType = AnalyzeBlockReturns(block, out var _, analyzeContext);
                    methodType.MainSignature.ReturnType = returnType;
                }
            }
        }
        else if (unResolved is UnResolvedSource unResolvedSource)
        {
            var block = unResolvedSource.Block;
            var returnType = AnalyzeBlockReturns(block, out var relatedExpr, analyzeContext);
            if (returnType is LuaMultiReturnType multiReturnType)
            {
                returnType = multiReturnType.GetElementType(0);
            }

            Compilation.ProjectIndex.AddModuleReturns(unResolvedSource.DocumentId, returnType, relatedExpr);
        }
    }

    private void ResolveParameters(UnResolved unResolved)
    {
        if (unResolved is UnResolvedClosureParameters unResolvedClosureParameters)
        {
            var callExpr = unResolvedClosureParameters.CallExpr;
            var prefixType = Context.Infer(callExpr.PrefixExpr);
            var callArgList = callExpr.ArgList?.ArgList.ToList() ?? [];
            foreach (var methodType1 in Context.FindCallableType(prefixType))
            {
                var signature = Context.FindPerfectMatchSignature(methodType1, callExpr, callArgList);
                var paramIndex = unResolvedClosureParameters.Index;
                if (paramIndex == -1) break;
                var paramDeclaration = signature.Parameters.ElementAtOrDefault(paramIndex);
                if (paramDeclaration is null) break;
                var closureParams = unResolvedClosureParameters.Parameters;
                if (Context.FindCallableType(paramDeclaration.Type).FirstOrDefault() is { } methodType2)
                {
                    var mainParams = methodType2.MainSignature.Parameters;
                    for (var i = 0; i < closureParams.Count && i < mainParams.Count; i++)
                    {
                        var closureParam = closureParams[i];
                        var mainParam = mainParams[i];
                        Compilation.TypeManager.SetBaseType(closureParam.UniqueId, mainParam.Type ?? Builtin.Any);
                    }
                }
            }
        }
    }

    private LuaType AnalyzeBlockReturns(LuaBlockSyntax mainBlock, out List<LuaExprSyntax> relatedExpr,
        AnalyzeContext analyzeContext)
    {
        LuaType returnType = Builtin.Unknown;
        relatedExpr = [];
        var cfg = analyzeContext.GetControlFlowGraph(mainBlock);
        if (cfg is null)
        {
            return Builtin.Nil;
        }

        var prevNodes = cfg.GetPredecessors(cfg.ExitNode).ToList();
        foreach (var prevNode in prevNodes)
        {
            if (prevNode.Statements.Count != 0)
            {
                if (prevNode.Statements.Last().ToNode(Context) is LuaReturnStatSyntax returnStmt)
                {
                    var rets = returnStmt.ExprList.ToList();
                    switch (rets.Count)
                    {
                        case 0:
                        {
                            returnType = returnType.Union(Builtin.Nil, Context);
                            break;
                        }
                        case >= 1:
                        {
                            var mainReturn = Context.Infer(rets[0]);
                            if (mainReturn.IsSameType(Builtin.Unknown, Context))
                            {
                                return returnType;
                            }

                            relatedExpr.Add(rets[0]);
                            returnType = returnType.Union(mainReturn, Context);
                            break;
                        }
                    }
                }
                else
                {
                    returnType = returnType.Union(Builtin.Nil, Context);
                }
            }
            else
            {
                returnType = returnType.Union(Builtin.Nil, Context);
            }
        }

        return returnType.IsSameType(Builtin.Unknown, Context) ? Builtin.Nil : returnType;
    }

    private void MergeType(UnResolvedSymbol unResolved, LuaExprSyntax luaExpr, LuaType type, int retId)
    {
        if (type is LuaMultiReturnType returnType)
        {
            type = returnType.GetElementType(retId);
        }
        else if (retId != 0)
        {
            type = Builtin.Nil;
        }

        if (luaExpr is LuaTableExprSyntax tableExprSyntax)
        {
            type = new LuaElementType(tableExprSyntax.UniqueId);
        }

        var declaration = unResolved.LuaSymbol;

        if (declaration.Type is null)
        {
            if (declaration.IsLocal && (type.IsSameType(Builtin.Table, Context) ||
                                        type.IsSameType(Builtin.UserData, Context)
                ))
            {
                declaration.Type = new LuaElementType(declaration.UniqueId);
                Context.Compilation.TypeManager.AddDocumentElementType(declaration.UniqueId);
                Context.Compilation.TypeManager.SetBaseType(declaration.UniqueId, type);
            }
            else if (luaExpr is LuaCallExprSyntax)
            {
                declaration.Type = new LuaElementType(declaration.UniqueId);
                Context.Compilation.TypeManager.AddDocumentElementType(declaration.UniqueId);
                Context.Compilation.TypeManager.SetBaseType(declaration.UniqueId, type);
            }
            else if (type is LuaElementType elementType)
            {
                // same file use ref
                if (elementType.Id.DocumentId == declaration.DocumentId)
                {
                    declaration.Type = elementType;
                }
                else
                {
                    declaration.Type = new LuaElementType(declaration.UniqueId);
                    Context.Compilation.TypeManager.AddDocumentElementType(declaration.UniqueId);
                    Context.Compilation.TypeManager.SetBaseType(declaration.UniqueId, elementType);
                }
            }
            else
            {
                declaration.Type = type;
            }
        }
        else
        {
            var declarationType = unResolved.LuaSymbol.Type;
            if (declarationType is LuaElementType elementType)
            {
                Compilation.TypeManager.SetBaseType(elementType.Id, type);
            }
            else if (declarationType is GlobalNameType globalNameType)
            {
                Compilation.TypeManager.SetExprType(luaExpr.DocumentId, globalNameType, type);
            }
            else if (declarationType is LuaNamedType namedType)
            {
                Compilation.TypeManager.SetExprType(namedType, type);
            }
        }
    }
}
