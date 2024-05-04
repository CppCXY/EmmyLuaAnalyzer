using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;

public class ResolveAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Resolve")
{
    private SearchContext Context { get; } = new(compilation, new SearchContextFeatures()
    {
        Cache = true,
        CacheUnknown = false,
        CacheBaseMember = false
    });

    public override void Analyze(AnalyzeContext analyzeContext)
    {
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
        Context.ClearCache();
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
                            for (var i = 0; i < unResolvedForRangeParameter.ParameterLuaDeclarations.Count; i++)
                            {
                                var parameter = unResolvedForRangeParameter.ParameterLuaDeclarations[i];
                                if (parameter.Info.DeclarationType is null)
                                {
                                    parameter.Info = parameter.Info with
                                    {
                                        DeclarationType = multiReturnType.GetElementType(i)
                                    };
                                }

                                ;
                            }
                        }
                        else if (unResolvedForRangeParameter.ParameterLuaDeclarations.FirstOrDefault() is
                                 { } firstDeclaration)
                        {
                            if (firstDeclaration.Info.DeclarationType is null)
                            {
                                firstDeclaration.Info = firstDeclaration.Info with
                                {
                                    DeclarationType = returnType
                                };
                            }

                            ;
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
                declaration.Info = declaration.Info with {DeclarationType = new LuaNamedType(declaration.Info.Ptr.Stringify)};
            }
        }
    }

    private void ResolveIndex(UnResolved unResolved)
    {
        if (unResolved is UnResolvedDeclaration unResolvedDeclaration)
        {
            var declaration = unResolvedDeclaration.LuaDeclaration;
            if (declaration.Info.Ptr.ToNode(Context) is LuaIndexExprSyntax {PrefixExpr: { } prefixExpr} indexExpr)
            {
                var documentId = indexExpr.DocumentId;
                var ty = Context.Infer(prefixExpr);
                if (ty is LuaNamedType namedType)
                {
                    Compilation.DbManager.AddMember(documentId, namedType.Name, declaration);
                }
            }
        }
    }

    private void ResolveReturn(UnResolved unResolved, AnalyzeContext analyzeContext)
    {
        if (unResolved is UnResolvedMethod unResolvedMethod)
        {
            var methodType = unResolvedMethod.MethodType;
            if (!methodType.MainSignature.ReturnType.Equals(Builtin.Unknown))
            {
                return;
            }

            var block = unResolvedMethod.Block;
            var returnType = AnalyzeBlockReturns(block, out var _, analyzeContext);
            methodType.MainSignature.ReturnType = returnType;
        }
        else if (unResolved is UnResolvedSource unResolvedSource)
        {
            var block = unResolvedSource.Block;
            var returnType = AnalyzeBlockReturns(block, out var relatedExpr, analyzeContext);
            if (returnType is LuaMultiReturnType multiReturnType)
            {
                returnType = multiReturnType.GetElementType(0);
            }

            Compilation.DbManager.AddModuleExport(unResolvedSource.DocumentId, returnType, relatedExpr);
        }
    }

    private void ResolveParameters(UnResolved unResolved)
    {
        if (unResolved is UnResolvedClosureParameters unResolvedClosureParameters)
        {
            var callExpr = unResolvedClosureParameters.CallExprSyntax;
            var prefixType = Context.Infer(callExpr.PrefixExpr);
            var callArgList = callExpr.ArgList?.ArgList.ToList() ?? [];
            TypeHelper.Each<LuaMethodType>(prefixType, type =>
            {
                var signature = type.FindPerfectMatchSignature(callExpr, callArgList, Context);
                var paramIndex = unResolvedClosureParameters.Index;
                if (paramIndex == -1) return;
                var paramDeclaration = signature.Parameters.ElementAtOrDefault(paramIndex);
                if (paramDeclaration is not {Info.DeclarationType: { } paramType}) return;
                var closureParams = unResolvedClosureParameters.ParameterLuaDeclarations;
                TypeHelper.Each<LuaMethodType>(paramType, methodType =>
                {
                    var mainParams = methodType.MainSignature.Parameters;
                    for (var i = 0; i < closureParams.Count && i < mainParams.Count; i++)
                    {
                        var closureParam = closureParams[i];
                        var mainParam = mainParams[i];
                        if (closureParam.Info.DeclarationType is null)
                        {
                            closureParam.Info = closureParam.Info with {DeclarationType = mainParam.Info.DeclarationType};
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
        relatedExpr = new List<LuaExprSyntax>();
        var cfg = analyzeContext.GetControlFlowGraph(mainBlock);
        if (cfg is null)
        {
            return returnType;
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
            declaration.Info = declaration.Info with {DeclarationType = type};
        }
        else if (unResolved.IsTypeDeclaration && TypeHelper.IsExtensionType(type))
        {
            var declarationType = unResolved.LuaDeclaration.Info.DeclarationType;
            if (declarationType is LuaNamedType namedType)
            {
                if (type is LuaTableLiteralType tableType)
                {
                    var typeName = namedType.Name;
                    var members = Compilation.DbManager.GetMembers(tableType.Name);
                    var documentId = declaration.Info.Ptr.DocumentId;

                    foreach (var member in members)
                    {
                        Compilation.DbManager.AddMember(documentId, typeName, member);
                    }

                    Compilation.DbManager.AddIdRelatedType(documentId, tableType.TableExprPtr.UniqueId, namedType);
                }
                else
                {
                    var documentId = declaration.Info.Ptr.DocumentId;
                    Compilation.DbManager.AddSuper(documentId, namedType.Name, type);
                }
            }
        }
    }
}
