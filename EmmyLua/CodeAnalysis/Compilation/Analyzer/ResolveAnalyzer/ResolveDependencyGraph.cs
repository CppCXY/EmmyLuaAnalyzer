using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;

public class ResolveDependencyGraph(SearchContext context, AnalyzeContext analyzeContext)
{
    private Dictionary<UnResolved, Dictionary<ResolveState, List<LuaExprSyntax>>> Dependencies { get; } = new();

    public delegate void ResolveStateHandler(UnResolved unResolved, ResolveState state);

    public event ResolveStateHandler? OnResolved;

    public event ResolveStateHandler? OnForceTypeResolved;

    private void CalcDependency()
    {
        bool changed;
        do
        {
            changed = false;
            var itemsToRemove = new List<(UnResolved, ResolveState)>();
            foreach (var (unResolved, dict) in Dependencies)
            {
                foreach (var (state, exprs) in dict)
                {
                    var allResolved = exprs.Select(context.Infer).All(ty => !ty.IsSameType(Builtin.Unknown, context));
                    if (allResolved)
                    {
                        OnResolved?.Invoke(unResolved, state);
                        changed = true;
                        itemsToRemove.Add((unResolved, state));
                    }
                }
            }

            foreach (var (unResolved, state) in itemsToRemove)
            {
                Dependencies[unResolved].Remove(state);
                if (Dependencies[unResolved].Count == 0)
                {
                    Dependencies.Remove(unResolved);
                }
            }
        } while (changed);
    }

    private void ForceResolveType()
    {
        var itemsToRemove = new List<(UnResolved, ResolveState)>();
        foreach (var (unResolved, dict) in Dependencies)
        {
            foreach (var (state, _) in dict)
            {
                if ((state & ResolveState.UnResolvedType) != 0)
                {
                    OnForceTypeResolved?.Invoke(unResolved, state);
                    itemsToRemove.Add((unResolved, state));
                }
            }
        }

        foreach (var (unResolved, state) in itemsToRemove)
        {
            Dependencies[unResolved].Remove(state);
            if (Dependencies[unResolved].Count == 0)
            {
                Dependencies.Remove(unResolved);
            }
        }
    }

    public void Resolve(List<UnResolved> unResolvedList)
    {
        foreach (var unResolved in unResolvedList)
        {
            if ((unResolved.ResolvedState & ResolveState.UnResolvedIndex) != 0)
            {
                CalcResolveIndex(unResolved);
            }

            if ((unResolved.ResolvedState & ResolveState.UnResolvedType) != 0)
            {
                CalcResolveType(unResolved);
            }

            if ((unResolved.ResolvedState & ResolveState.UnResolveReturn) != 0)
            {
                CalcResolveReturn(unResolved);
            }

            if ((unResolved.ResolvedState & ResolveState.UnResolvedParameters) != 0)
            {
                CalcResolveParameters(unResolved);
            }
        }

        var forceType = false;
        do
        {
            CalcDependency();
            if (!forceType)
            {
                forceType = true;
                ForceResolveType();
            }
            else
            {
                break;
            }
        } while (Dependencies.Count != 0);
    }

    private void AddDependency(UnResolved unResolved, ResolveState state, LuaExprSyntax expr)
    {
        if (!Dependencies.TryGetValue(unResolved, out var dict))
        {
            dict = new Dictionary<ResolveState, List<LuaExprSyntax>>();
            Dependencies[unResolved] = dict;
        }

        if (!dict.TryGetValue(state, out var list))
        {
            list = [];
            dict[state] = list;
        }

        list.Add(expr);
    }

    private void CalcResolveType(UnResolved unResolved)
    {
        if (unResolved is UnResolvedSymbol unResolvedDeclaration)
        {
            var exprRef = unResolvedDeclaration.ExprRef;
            if (exprRef is not null)
            {
                var exprType = context.Infer(exprRef.Expr);
                if (!exprType.IsSameType(Builtin.Unknown, context))
                {
                    OnResolved?.Invoke(unResolved, ResolveState.UnResolvedType);
                }
                else
                {
                    AddDependency(unResolved, ResolveState.UnResolvedType, exprRef.Expr);
                }
            }
        }
        else if (unResolved is UnResolvedForRangeParameter unResolvedForRangeParameter)
        {
            var exprList = unResolvedForRangeParameter.ExprList;
            switch (exprList.Count)
            {
                // ipairs and pairs
                case 1:
                {
                    var iterExpr = exprList.First();
                    var iterType = context.Infer(iterExpr);
                    if (!iterType.IsSameType(Builtin.Unknown, context))
                    {
                        OnResolved?.Invoke(unResolved, ResolveState.UnResolvedType);
                    }
                    else
                    {
                        AddDependency(unResolved, ResolveState.UnResolvedType, iterExpr);
                    }

                    return;
                }
                // custom iterator
                default:
                {
                    OnResolved?.Invoke(unResolved, ResolveState.UnResolvedType);
                    break;
                }
            }
        }
    }

    private void CalcResolveIndex(UnResolved unResolved)
    {
        if (unResolved is UnResolvedSymbol unResolvedDeclaration)
        {
            var declaration = unResolvedDeclaration.LuaSymbol;
            if (declaration.Info.Ptr.ToNode(context) is LuaIndexExprSyntax { PrefixExpr: { } prefixExpr })
            {
                var ty = context.Infer(prefixExpr);
                if (!ty.IsSameType(Builtin.Unknown, context))
                {
                    OnResolved?.Invoke(unResolved, ResolveState.UnResolvedIndex);
                }
                else
                {
                    AddDependency(unResolved, ResolveState.UnResolvedIndex, prefixExpr);
                }
            }
        }
    }

    private void CalcResolveReturn(UnResolved unResolved)
    {
        if (unResolved is UnResolvedMethod unResolvedMethod)
        {
            var id = unResolvedMethod.Id;
            var typeInfo = context.Compilation.TypeManager.FindTypeInfo(id);
            if (typeInfo?.BaseType is LuaMethodType methodType)
            {
                // var retType = methodType.MainSignature.ReturnType;
                // if (!retType.IsSameType(Builtin.Unknown, context))
                // {
                //     OnResolved?.Invoke(unResolved, ResolveState.UnResolveReturn);
                // }
                // else
                // {
                //     var block = unResolvedMethod.Block;
                //     AnalyzeBlockReturns(block, unResolved);
                // }
            }
        }
        else if (unResolved is UnResolvedSource unResolvedSource)
        {
            var block = unResolvedSource.Block;
            AnalyzeBlockReturns(block, unResolved);
        }
    }

    private void CalcResolveParameters(UnResolved unResolved)
    {
        if (unResolved is UnResolvedClosureParameters { CallExpr.PrefixExpr: { } prefixExpr })
        {
            var prefixType = context.Infer(prefixExpr);
            if (!prefixType.IsSameType(Builtin.Unknown, context))
            {
                OnResolved?.Invoke(unResolved, ResolveState.UnResolvedParameters);
            }
            else
            {
                AddDependency(unResolved, ResolveState.UnResolvedParameters, prefixExpr);
            }
        }
    }

    private void AnalyzeBlockReturns(LuaBlockSyntax mainBlock, UnResolved unResolved)
    {
        var cfg = analyzeContext.GetControlFlowGraph(mainBlock);
        if (cfg is null)
        {
            return;
        }

        var canResolve = true;
        var prevNodes = cfg.GetPredecessors(cfg.ExitNode).ToList();
        foreach (var prevNode in prevNodes)
        {
            if (prevNode.Statements.Count != 0)
            {
                if (prevNode.Statements.Last().ToNode(context) is LuaReturnStatSyntax returnStmt)
                {
                    var rets = returnStmt.ExprList.ToList();
                    switch (rets.Count)
                    {
                        case 0:
                        {
                            break;
                        }
                        case >= 1:
                        {
                            var mainReturn = context.Infer(rets[0]);
                            if (mainReturn.IsSameType(Builtin.Unknown, context))
                            {
                                canResolve = false;
                                AddDependency(unResolved, ResolveState.UnResolveReturn, rets[0]);
                            }

                            break;
                        }
                    }
                }
            }
        }

        if (canResolve)
        {
            OnResolved?.Invoke(unResolved, ResolveState.UnResolveReturn);
        }
    }
}
