using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;
using EmmyLua.CodeAnalysis.Compile.Diagnostic;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Bind;

public class BindAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    private SearchContext Context => Compilation.SearchContext;
    private Dictionary<DocumentId, BindData> BindData { get; } = new();

    public override void Analyze(DocumentId documentId)
    {
        if (BindData.ContainsKey(documentId))
        {
            return;
        }

        var declarationTree = Compilation.GetDeclarationTree(documentId);
        if (declarationTree is null)
        {
            return;
        }

        var bindData = new BindData(documentId, declarationTree);
        BindData.Add(documentId, bindData);

        if (Compilation.GetSyntaxTree(documentId) is { } syntaxTree)
        {
            foreach (var node in syntaxTree.SyntaxRoot.Descendants)
            {
                switch (node)
                {
                    case LuaLocalStatSyntax luaLocalStat:
                    {
                        LocalBindAnalysis(luaLocalStat, bindData);
                        break;
                    }
                    case LuaAssignStatSyntax luaAssignStat:
                    {
                        AssignBindAnalysis(luaAssignStat, bindData);
                        break;
                    }
                    case LuaForStatSyntax luaForStat:
                    {
                        ForStatBindAnalysis(luaForStat, bindData);
                        break;
                    }
                    case LuaForRangeStatSyntax luaForRangeStat:
                    {
                        ForRangeBindAnalysis(luaForRangeStat, bindData);
                        break;
                    }
                }
            }
        }

        bindData.Step = BindAnalyzeStep.Finish;
    }

    private void LocalBindAnalysis(LuaLocalStatSyntax localStat, BindData bindData)
    {
        var tree = bindData.Tree;
        var nameList = localStat.NameList.ToList();
        var exprList = localStat.ExprList.ToList();
        var count = nameList.Count;
        ILuaType currentExprType = Context.Compilation.Builtin.Unknown;
        var retId = 0;
        for (var i = 0; i < count; i++)
        {
            var localName = nameList[i];
            var expr = exprList.ElementAtOrDefault(i);
            if (expr is not null)
            {
                currentExprType = Compilation.SearchContext.Infer(expr);
                retId = 0;
            }
            else
            {
                retId++;
            }

            ILuaType exprType = Context.Compilation.Builtin.Unknown;
            if (currentExprType is LuaMultiRetType multiRetType)
            {
                exprType = multiRetType.GetRetType(retId) ?? exprType;
            }
            else if (retId == 0)
            {
                exprType = currentExprType;
            }

            var declaration = tree.FindDeclaration(localName);
            if (declaration is { Type: { } ty })
            {
                if (!exprType.SubTypeOf(ty, Context))
                {
                    localStat.Tree.PushDiagnostic(new Diagnostic(
                        DiagnosticSeverity.Warning,
                        DiagnosticCode.TypeNotMatch,
                        $"Local variable '{localName.Name?.RepresentText}' is type '{ty.ToDisplayString(Context)}' not match expr type '{exprType.ToDisplayString(Context)}'",
                        localName.Location
                    ));
                }
            }
            else
            {
                if (declaration != null) declaration.Type = exprType;
            }
        }
    }

    private void AssignBindAnalysis(LuaAssignStatSyntax assignStat, BindData bindData)
    {
        var tree = bindData.Tree;
        var varList = assignStat.VarList.ToList();
        var exprList = assignStat.ExprList.ToList();
        var count = varList.Count;
        ILuaType currentExprType = Context.Compilation.Builtin.Unknown;
        var retId = 0;
        for (var i = 0; i < count; i++)
        {
            var var = varList[i];
            var expr = exprList.ElementAtOrDefault(i);
            if (expr is not null)
            {
                currentExprType = Compilation.SearchContext.Infer(expr);
                retId = 0;
            }
            else
            {
                retId++;
            }

            ILuaType exprType = Context.Compilation.Builtin.Unknown;
            if (currentExprType is LuaMultiRetType multiRetType)
            {
                exprType = multiRetType.GetRetType(retId) ?? exprType;
            }
            else if (retId == 0)
            {
                exprType = currentExprType;
            }

            var declaration = tree.FindDeclaration(var);
            if (declaration is { Type: { } ty })
            {
                if (!exprType.SubTypeOf(ty, Context))
                {
                    assignStat.Tree.PushDiagnostic(new Diagnostic(
                        DiagnosticSeverity.Warning,
                        DiagnosticCode.TypeNotMatch,
                        $"Variable {var} is type '{ty.ToDisplayString(Context)}' not match '{exprType.ToDisplayString(Context)}'",
                        var.Location
                    ));
                }
            }
            else
            {
                if (declaration != null) declaration.Type = exprType;
            }
        }
    }

    private void ForStatBindAnalysis(LuaForStatSyntax forStat, BindData bindData)
    {
        var tree = bindData.Tree;
        if (forStat.IteratorName is { } itName)
        {
            var declaration = tree.FindDeclaration(itName);
            if (declaration is { Type: { } ty })
            {
                if (forStat.InitExpr is { } initExpr)
                {
                    var initTy = Context.Infer(initExpr);
                    if (!initTy.SubTypeOf(ty, Context))
                    {
                        forStat.Tree.PushDiagnostic(new Diagnostic(
                            DiagnosticSeverity.Warning,
                            DiagnosticCode.TypeNotMatch,
                            "The initialization expression of the for statement must be an integer",
                            initExpr.Location
                        ));
                    }
                }

                if (forStat.LimitExpr is { } limitExpr)
                {
                    var limitTy = Context.Infer(limitExpr);
                    if (!limitTy.SubTypeOf(ty, Context))
                    {
                        forStat.Tree.PushDiagnostic(new Diagnostic(
                            DiagnosticSeverity.Warning,
                            DiagnosticCode.TypeNotMatch,
                            "The limit expression of the for statement must be an integer",
                            limitExpr.Location
                        ));
                    }
                }

                if (forStat.Step is { } step)
                {
                    var stepTy = Context.Infer(step);
                    if (!stepTy.SubTypeOf(ty, Context))
                    {
                        forStat.Tree.PushDiagnostic(new Diagnostic(
                            DiagnosticSeverity.Warning,
                            DiagnosticCode.TypeNotMatch,
                            "The step expression of the for statement must be an integer",
                            step.Location
                        ));
                    }
                }
            }
        }
    }

    private void ForRangeBindAnalysis(LuaForRangeStatSyntax forRangeStat, BindData bindData)
    {
        var tree = bindData.Tree;
        var iterNames = forRangeStat.IteratorNames.ToList();
        var iterExpr = forRangeStat.ExprList.ToList().FirstOrDefault();
        var iterExprType = Context.Infer(iterExpr);
        if (iterExprType is LuaMethod luaMethod)
        {
            var multiReturn = LuaMultiRetType.FromType(luaMethod.ReturnType);
            var tyList = multiReturn.Returns;
            var count = iterNames.Count;
            for (var i = 0; i < count; i++)
            {
                var iterName = iterNames[i];
                var ty = tyList.ElementAtOrDefault(i) ?? Context.Compilation.Builtin.Unknown;

                var declaration = tree.FindDeclaration(iterName);
                if (declaration is { Type: { } declTy })
                {
                    if (!ty.SubTypeOf(declTy, Context))
                    {
                        forRangeStat.Tree.PushDiagnostic(new Diagnostic(
                            DiagnosticSeverity.Warning,
                            DiagnosticCode.TypeNotMatch,
                            $"The type {declTy.ToDisplayString(Context)} of the iterator variable {iterName} does not match ${ty.ToDisplayString(Context)}",
                            iterName.Location
                        ));
                    }
                }
                else
                {
                    if (declaration != null) declaration.Type = ty;
                }
            }
        }
        else if (iterExpr is not null)
        {
            forRangeStat.Tree.PushDiagnostic(new Diagnostic(
                DiagnosticSeverity.Warning,
                DiagnosticCode.TypeNotMatch,
                "The expression of the for-range-statement must return a function or be a function",
                iterExpr.Location
            ));
        }
    }

    public override void RemoveCache(DocumentId documentId)
    {
        BindData.Remove(documentId);
    }
}
