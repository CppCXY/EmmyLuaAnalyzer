using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
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
        if (unResolved is UnResolvedDeclaration unResolvedDeclaration)
        {
            var exprRef = unResolvedDeclaration.ExprRef;
            if (exprRef is not null)
            {
                var exprType = Context.Infer(exprRef.Expr);
                if (!exprType.Equals(Builtin.Unknown))
                {
                    MergeType(unResolvedDeclaration, exprType, exprRef.RetId);
                }
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
                                if (parameter.Info.DeclarationType is LuaVariableRefType luaVariableRefType)
                                {
                                    Context.Compilation.Db.UpdateIdRelatedType(luaVariableRefType.Id,
                                        multiReturnType.GetElementType(i));
                                }
                            }
                        }
                        else if (unResolvedForRangeParameter.Parameters.FirstOrDefault() is
                                 { } firstDeclaration)
                        {
                            if (firstDeclaration.Info.DeclarationType is LuaVariableRefType luaVariableRefType)
                            {
                                Context.Compilation.Db.UpdateIdRelatedType(luaVariableRefType.Id,
                                    returnType);
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
        if (unResolved is UnResolvedDeclaration unResolvedDeclaration)
        {
            var declaration = unResolvedDeclaration.LuaDeclaration;
            if (declaration.Info.DeclarationType is null)
            {
                declaration.Info = declaration.Info with
                {
                    DeclarationType = new LuaNamedType(declaration.Info.Ptr.Stringify)
                };
            }
        }
    }

    private void ResolveIndex(UnResolved unResolved)
    {
        if (unResolved is UnResolvedDeclaration unResolvedDeclaration)
        {
            var declaration = unResolvedDeclaration.LuaDeclaration;
            if (declaration.Info.Ptr.ToNode(Context) is LuaIndexExprSyntax { PrefixExpr: { } prefixExpr } indexExpr)
            {
                var documentId = indexExpr.DocumentId;
                var ty = Context.Infer(prefixExpr);
                Compilation.Db.AddMember(documentId, ty, declaration);
                Context.ClearMemberCache(ty);
            }
        }
    }

    private void ResolveReturn(UnResolved unResolved, AnalyzeContext analyzeContext)
    {
        if (unResolved is UnResolvedMethod unResolvedMethod)
        {
            var id = unResolvedMethod.Id;
            var idType = Context.Compilation.Db.QueryTypeFromId(id);
            if (idType is LuaMethodType methodType)
            {
                if (!methodType.MainSignature.ReturnType.Equals(Builtin.Unknown))
                {
                    return;
                }

                var block = unResolvedMethod.Block;
                var returnType = AnalyzeBlockReturns(block, out var _, analyzeContext);
                methodType.MainSignature.ReturnType = returnType;
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

            Compilation.Db.AddModuleReturns(unResolvedSource.DocumentId, returnType, relatedExpr);
        }
    }

    private void ResolveParameters(UnResolved unResolved)
    {
        if (unResolved is UnResolvedClosureParameters unResolvedClosureParameters)
        {
            var callExpr = unResolvedClosureParameters.CallExpr;
            var prefixType = Context.Infer(callExpr.PrefixExpr);
            var callArgList = callExpr.ArgList?.ArgList.ToList() ?? [];
            Context.FindMethodsForType(prefixType, type =>
            {
                var signature = Context.FindPerfectMatchSignature(type, callExpr, callArgList);
                var paramIndex = unResolvedClosureParameters.Index;
                if (paramIndex == -1) return;
                var paramDeclaration = signature.Parameters.ElementAtOrDefault(paramIndex);
                if (paramDeclaration is null) return;
                var closureParams = unResolvedClosureParameters.Parameters;
                Context.FindMethodsForType(paramDeclaration.Type, methodType =>
                {
                    var mainParams = methodType.MainSignature.Parameters;
                    for (var i = 0; i < closureParams.Count && i < mainParams.Count; i++)
                    {
                        var closureParam = closureParams[i];
                        var mainParam = mainParams[i];
                        if (closureParam.Info.DeclarationType is null)
                        {
                            closureParam.Info = closureParam.Info with { DeclarationType = mainParam.Type };
                        }
                    }
                });
            });
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
                            returnType = returnType.Union(Builtin.Nil);
                            break;
                        }
                        case >= 1:
                        {
                            var mainReturn = Context.Infer(rets[0]);
                            if (mainReturn.Equals(Builtin.Unknown))
                            {
                                return returnType;
                            }

                            relatedExpr.Add(rets[0]);
                            returnType = returnType.Union(mainReturn);
                            break;
                        }
                    }
                }
                else
                {
                    returnType = returnType.Union(Builtin.Nil);
                }
            }
            else
            {
                returnType = returnType.Union(Builtin.Nil);
            }
        }

        return returnType;
    }

    private void MergeType(UnResolvedDeclaration unResolved, LuaType type, int retId)
    {
        if (type is LuaMultiReturnType returnType)
        {
            type = returnType.GetElementType(retId);
        }
        else if (retId != 0)
        {
            type = Builtin.Nil;
        }

        var declaration = unResolved.LuaDeclaration;

        if (declaration.Info.DeclarationType is null)
        {
            declaration.Info = declaration.Info with { DeclarationType = type };
        }
        else
        {
            var declarationType = unResolved.LuaDeclaration.Info.DeclarationType;
            if (declarationType is LuaVariableRefType variableRefType)
            {
                Compilation.Db.UpdateIdRelatedType(variableRefType.Id, type);
            }
            else if (declarationType is GlobalNameType globalNameType)
            {
                Compilation.Db.AddGlobalRelationType(declaration.DocumentId, globalNameType.Name, type);
            }
            else if (declarationType is LuaNamedType namedType)
            {
                if (type is LuaTableLiteralType tableType)
                {
                    var members = Compilation.Db.QueryMembers(tableType).OfType<LuaDeclaration>();
                    var documentId = declaration.DocumentId;

                    foreach (var member in members)
                    {
                        Compilation.Db.AddMember(documentId, namedType, member);
                    }

                    Compilation.Db.UpdateIdRelatedType(tableType.TableExprPtr.UniqueId, namedType);
                }
                else if (type.IsExtensionType())
                {
                    Compilation.Db.AddSuper(declaration.DocumentId, namedType.Name, type);
                }
            }
        }
    }
}
