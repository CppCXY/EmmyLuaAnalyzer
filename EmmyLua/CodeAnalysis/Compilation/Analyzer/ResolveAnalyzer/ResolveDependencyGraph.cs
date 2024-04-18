using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;

public class ResolveDependencyGraph(SearchContext context)
{
    public readonly struct CanResolved(UnResolved unResolved, ResolveState state)
    {
        public UnResolved UnResolved { get; } = unResolved;

        public ResolveState State { get; } = state;
    }

    private Dictionary<UnResolved, Dictionary<ResolveState, List<LuaExprSyntax>>> Dependencies { get; } = new();

    private Queue<CanResolved> CanResolvedQueue { get; } = new();

    public IEnumerable<CanResolved> CanResolvedList
    {
        get
        {
            while (CanResolvedQueue.Count != 0)
            {
                yield return CanResolvedQueue.Dequeue();
            }
        }
    }

    public IEnumerable<CanResolved> UnResolvedList
    {
        get
        {
            foreach (var (unResolved, dict) in Dependencies)
            {
                foreach (var (state, _) in dict)
                {
                    yield return new CanResolved(unResolved, state);
                }
            }
        }
    }

    public bool CalcDependency()
    {
        var changed = false;
        var itemsToRemove = new List<(UnResolved, ResolveState)>();
        foreach (var (unResolved, dict) in Dependencies)
        {
            foreach (var (state, exprs) in dict)
            {
                var allResolved = exprs.Select(context.Infer).All(ty => !ty.Equals(Builtin.Unknown));
                if (allResolved)
                {
                    CanResolvedQueue.Enqueue(new CanResolved(unResolved, state));
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

        return changed;
    }

    public void Build(List<UnResolved> unResolvedList)
    {
        // index first
        foreach (var unResolved in unResolvedList)
        {
            if ((unResolved.ResolvedState & ResolveState.UnResolvedIndex) != 0)
            {
                CalcResolveIndex(unResolved);
            }
        }
        // other
        foreach (var unResolved in unResolvedList)
        {
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
            list = new List<LuaExprSyntax>();
            dict[state] = list;
        }

        list.Add(expr);
    }

    private void CalcResolveType(UnResolved unResolved)
    {
        if (unResolved is UnResolvedDeclaration unResolvedDeclaration)
        {
            var exprRef = unResolvedDeclaration.ExprRef;
            if (exprRef is not null)
            {
                var exprType = context.Infer(exprRef.Expr);
                if (!exprType.Equals(Builtin.Unknown))
                {
                    CanResolvedQueue.Enqueue(new CanResolved(unResolved, ResolveState.UnResolvedType));
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
                    if (!iterType.Equals(Builtin.Unknown))
                    {
                        CanResolvedQueue.Enqueue(new CanResolved(unResolved, ResolveState.UnResolvedType));
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
                    CanResolvedQueue.Enqueue(new CanResolved(unResolved, ResolveState.UnResolvedType));
                    break;
                }
            }
        }
    }

    private void CalcResolveIndex(UnResolved unResolved)
    {
        if (unResolved is UnResolvedDeclaration unResolvedDeclaration)
        {
            var declaration = unResolvedDeclaration.LuaDeclaration;
            if (declaration.Ptr.ToNode(context) is LuaIndexExprSyntax { PrefixExpr: { } prefixExpr } indexExpr)
            {
                var ty = context.Infer(prefixExpr);
                if (ty.Equals(Builtin.Unknown))
                {
                    AddDependency(unResolved, ResolveState.UnResolvedIndex, prefixExpr);
                }
                else
                {
                    CanResolvedQueue.Enqueue(new CanResolved(unResolved, ResolveState.UnResolvedIndex));
                }
            }
        }
    }

    private void CalcResolveReturn(UnResolved unResolved)
    {
        if (unResolved is UnResolvedMethod unResolvedMethod)
        {
            var methodType = unResolvedMethod.MethodType;
            if (!methodType.MainSignature.ReturnType.Equals(Builtin.Unknown))
            {
                CanResolvedQueue.Enqueue(new CanResolved(unResolved, ResolveState.UnResolveReturn));
                return;
            }

            var block = unResolvedMethod.Block;
            AnalyzeBlockReturns(block, unResolved);
        }
        else if (unResolved is UnResolvedSource unResolvedSource)
        {
            var block = unResolvedSource.Block;
            AnalyzeBlockReturns(block, unResolved);
        }
    }

    private void CalcResolveParameters(UnResolved unResolved)
    {
        if (unResolved is UnResolvedClosureParameters unResolvedClosureParameters)
        {
            var callExprSyntax = unResolvedClosureParameters.CallExprSyntax;
            if (callExprSyntax.PrefixExpr is { } prefixExpr)
            {
                var prefixType = context.Infer(prefixExpr);
                if (prefixType.Equals(Builtin.Unknown))
                {
                    AddDependency(unResolved, ResolveState.UnResolvedParameters, prefixExpr);
                }
                else
                {
                    CanResolvedQueue.Enqueue(new CanResolved(unResolved, ResolveState.UnResolvedParameters));
                }
            }
        }
    }

    private void AnalyzeBlockReturns(LuaBlockSyntax mainBlock, UnResolved unResolved)
    {
        var cfg = context.Compilation.GetControlFlowGraph(mainBlock);
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
                        case 1:
                        {
                            var mainReturn = context.Infer(rets[0]);
                            if (mainReturn.Equals(Builtin.Unknown))
                            {
                                canResolve = false;
                                AddDependency(unResolved, ResolveState.UnResolveReturn,rets[0]);
                            }

                            break;
                        }
                        case > 1:
                        {
                            foreach (var ret in rets)
                            {
                                var retType = context.Infer(ret);
                                if (retType.Equals(Builtin.Unknown))
                                {
                                    canResolve = false;
                                    AddDependency(unResolved, ResolveState.UnResolveReturn, ret);
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }

        if (canResolve)
        {
            CanResolvedQueue.Enqueue(new CanResolved(unResolved, ResolveState.UnResolveReturn));
        }
    }
}
