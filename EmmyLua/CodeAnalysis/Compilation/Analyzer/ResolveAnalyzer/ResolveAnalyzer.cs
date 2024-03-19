using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;

public class ResolveAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    public override void Analyze(AnalyzeContext analyzeContext)
    {
        bool changed;
        do
        {
            changed = false;
            var resolvedCount = 0;
            for (var i = 0; i < analyzeContext.UnResolves.Count - resolvedCount;)
            {
                var unResolved = analyzeContext.UnResolves[i];
                if ((unResolved.ResolvedState & ResolveState.UnResolvedType) != 0)
                {
                    ResolveType(unResolved, ref changed);
                }
                else if ((unResolved.ResolvedState & ResolveState.UnResolvedIndex) != 0)
                {
                    ResolveIndex(unResolved, ref changed);
                }
                else if ((unResolved.ResolvedState & ResolveState.UnResolveReturn) != 0)
                {
                    ResolveReturn(unResolved, ref changed);
                }

                if (unResolved.ResolvedState == ResolveState.Resolved)
                {
                    if (i < analyzeContext.UnResolves.Count - resolvedCount - 1)
                    {
                        // Move the resolved object to the end of the list
                        (analyzeContext.UnResolves[i], analyzeContext.UnResolves[analyzeContext.UnResolves.Count - resolvedCount - 1]) = (
                            analyzeContext.UnResolves[analyzeContext.UnResolves.Count - resolvedCount - 1],
                            analyzeContext.UnResolves[i]);
                    }
                    resolvedCount++;
                }
                else
                {
                    i++;
                }
            }
        } while (changed);
    }

    private void ResolveType(UnResolved unResolved, ref bool changed)
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
                    unResolved.ResolvedState &= ~ResolveState.UnResolvedType;
                    changed = true;
                }
            }
        }
        else if(unResolved is UnResolvedForRangeParameter unResolvedForRangeParameter)
        {
            var exprList = unResolvedForRangeParameter.ExprList;
            switch (exprList.Count)
            {
                case 0:
                {
                    changed = true;
                    unResolved.ResolvedState &= ~ResolveState.UnResolvedType;
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
                        var returnTypes = new List<LuaType>();
                        if (returnType is LuaMultiReturnType multiReturnType)
                        {
                            returnTypes.AddRange(multiReturnType.RetTypes);
                        }
                        else
                        {
                            returnTypes.Add(returnType);
                        }
                        for (var i = 0; i < unResolvedForRangeParameter.ParameterLuaDeclarations.Count; i++)
                        {
                            var parameter = unResolvedForRangeParameter.ParameterLuaDeclarations[i];
                            if (parameter.DeclarationType is null)
                            {
                                parameter.DeclarationType = i < returnTypes.Count ? returnTypes[i] : Builtin.Unknown;
                            }
                        }

                        changed = true;
                        unResolved.ResolvedState &= ~ResolveState.UnResolvedType;
                    }

                    return;
                }
                // custom iterator
                default:
                {
                    // TODO: implement custom iterator
                    changed = true;
                    unResolved.ResolvedState &= ~ResolveState.UnResolvedType;
                    break;
                }
            }
        }
    }

    private void ResolveIndex(UnResolved unResolved, ref bool changed)
    {
        if (unResolved is UnResolvedDeclaration unResolvedDeclaration)
        {
            var declaration = unResolvedDeclaration.LuaDeclaration;
            if (declaration.SyntaxElement is LuaIndexExprSyntax { PrefixExpr: { } prefixExpr } indexExpr)
            {
                var documentId = indexExpr.Tree.Document.Id;
                var ty = Context.Infer(prefixExpr);
                if (ty is LuaNamedType namedType)
                {
                    Compilation.ProjectIndex.AddMember(documentId, namedType.Name, declaration);
                    unResolved.ResolvedState &= ~ResolveState.UnResolvedIndex;
                    changed = true;
                }
            }
        }
    }

    private void ResolveReturn(UnResolved unResolved, ref bool changed)
    {
        if (unResolved is UnResolvedMethod unResolvedMethod)
        {
            var methodType = unResolvedMethod.MethodType;
            if (!methodType.MainSignature.ReturnType.Equals(Builtin.Unknown))
            {
                unResolved.ResolvedState &= ~ResolveState.UnResolveReturn;
                changed = true;
                return;
            }

            var block = unResolvedMethod.Block;
            var complete = false;
            var returnType = AnalyzeBlockReturns(block, ref complete);
            if (!complete)
            {
                return;
            }

            methodType.MainSignature.ReturnType = returnType;
            unResolved.ResolvedState &= ~ResolveState.UnResolveReturn;
            changed = true;
        }
        else if (unResolved is UnResolvedSource unResolvedSource)
        {
            var block = unResolvedSource.Block;
            var complete = false;
            var returnType = AnalyzeBlockReturns(block, ref complete);
            if (!complete)
            {
                return;
            }

            if (returnType is LuaMultiReturnType multiReturnType)
            {
                var mainReturn = multiReturnType.RetTypes.ElementAtOrDefault(0);
                if (mainReturn is not null)
                {
                    returnType = mainReturn;
                }
            }

            Compilation.ProjectIndex.AddExportType(unResolvedSource.DocumentId, returnType);
            unResolved.ResolvedState &= ~ResolveState.UnResolveReturn;
            changed = true;
        }
    }

    private LuaType AnalyzeBlockReturns(LuaBlockSyntax mainBlock, ref bool complete)
    {
        LuaType returnType = Builtin.Unknown;
        var cfg = Context.Compilation.GetControlFlowGraph(mainBlock);
        if (cfg is null)
        {
            return returnType;
        }

        var prevNodes = cfg.GetPredecessors(cfg.ExitNode).ToList();
        foreach (var prevNode in prevNodes)
        {
            if (prevNode.Statements.Count != 0)
            {
                if (prevNode.Statements.Last() is LuaReturnStatSyntax returnStmt)
                {
                    var rets = returnStmt.ExprList.ToList();
                    switch (rets.Count)
                    {
                        case 0:
                        {
                            returnType = returnType.Union(Builtin.Nil);
                            break;
                        }
                        case 1:
                        {
                            var mainReturn = Context.Infer(rets[0]);
                            if (mainReturn.Equals(Builtin.Unknown))
                            {
                                complete = false;
                                return returnType;
                            }

                            returnType = returnType.Union(mainReturn);
                            break;
                        }
                        case > 1:
                        {
                            var retTypes = new List<LuaType>();
                            foreach (var ret in rets)
                            {
                                var retType = Context.Infer(ret);
                                if (retType.Equals(Builtin.Unknown))
                                {
                                    complete = false;
                                    return returnType;
                                }

                                retTypes.Add(retType);
                            }

                            returnType = returnType.Union(new LuaMultiReturnType(retTypes));
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

        complete = true;
        return returnType;
    }

    private void MergeType(UnResolvedDeclaration unResolved, LuaType type, int retId)
    {
        if (type is LuaMultiReturnType returnType)
        {
            var childTy = returnType.RetTypes.ElementAtOrDefault(retId);
            if (childTy is not null)
            {
                type = childTy;
            }
        }

        var declaration = unResolved.LuaDeclaration;

        if (declaration.DeclarationType is null)
        {
            declaration.DeclarationType = type;
        }
        else if (unResolved.IsTypeDeclaration && type is LuaTableLiteralType tableLiteralType)
        {
            var members = Compilation.ProjectIndex.GetMembers(tableLiteralType.TableId);
            var documentId = declaration.SyntaxElement?.Tree.Document.Id;
            if (documentId is { } id)
            {
                foreach (var member in members)
                {
                    Compilation.ProjectIndex.AddMember(id, member.Name, member);
                }
            }
        }
    }
}
