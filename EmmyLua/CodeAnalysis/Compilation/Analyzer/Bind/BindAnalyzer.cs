using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compile.Diagnostic;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;

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

        var declarationTree = Compilation.GetSymbolTree(documentId);
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
                    case LuaCallExprSyntax luaCallExpr:
                    {
                        CallExprAnalysis(luaCallExpr, bindData);
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

            var symbol = tree.FindDeclaration(localName, Context);
            if (symbol is { DeclarationType: { } ty })
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
                if (symbol != null) symbol.DeclarationType = exprType;
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

            var symbol = tree.FindDeclaration(var, Context);
            if (symbol is { DeclarationType: { } ty })
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
                if (symbol != null) symbol.DeclarationType = exprType;
            }
        }
    }

    private void ForStatBindAnalysis(LuaForStatSyntax forStat, BindData bindData)
    {
        var tree = bindData.Tree;
        if (forStat.IteratorName is { } itName)
        {
            var symbol = tree.FindDeclaration(itName, Context);
            if (symbol is { DeclarationType: { } ty })
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
        if (iterExprType is LuaMethod { MainSignature: { } signature })
        {
            var multiReturn = LuaMultiRetType.FromType(signature.ReturnTypes);
            var tyList = multiReturn.Returns;
            var count = iterNames.Count;
            for (var i = 0; i < count; i++)
            {
                var iterName = iterNames[i];
                var ty = tyList.ElementAtOrDefault(i) ?? Context.Compilation.Builtin.Unknown;

                var declaration = tree.FindDeclaration(iterName, Context);
                if (declaration is { DeclarationType: { } declTy })
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
                    if (declaration != null) declaration.DeclarationType = ty;
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

    private void CheckFuncCallParams(
        LuaCallExprSyntax callExprSyntax,
        List<Symbol.Symbol> parameters,
        List<LuaExprSyntax> arguments
    )
    {
        var count = parameters.Count;
        var argCount = arguments.Count;
        for (var i = 0; i < count; i++)
        {
            var param = parameters[i];
            var arg = arguments.ElementAtOrDefault(i);
            if (arg is null)
            {
                if (param.DeclarationType is null or { IsNullable: true })
                {
                    continue;
                }
                else
                {
                    callExprSyntax.Tree.PushDiagnostic(new Diagnostic(
                        DiagnosticSeverity.Warning,
                        DiagnosticCode.MissingParameter,
                        $"The number of parameters passed in is less than the number of parameters required by the function",
                        callExprSyntax.ArgList?.RightParen?.Location ?? callExprSyntax.Location
                    ));
                    return;
                }
            }

            var argTy = Context.Infer(arg);
            if (param.DeclarationType is { } type)
            {
                if (!argTy.SubTypeOf(type, Context))
                {
                    callExprSyntax.Tree.PushDiagnostic(new Diagnostic(
                        DiagnosticSeverity.Warning,
                        DiagnosticCode.TypeNotMatch,
                        $"The type '{argTy.ToDisplayString(Context)}' of the argument does not match the type '{type.ToDisplayString(Context)}' of the parameter",
                        arg.Location
                    ));
                }
            }
            else
            {
                param.DeclarationType = argTy;
            }
        }
    }

    private void CallExprAnalysis(LuaCallExprSyntax callExpr, BindData bindData)
    {
        var prefixTy = Context.Infer(callExpr.PrefixExpr);
        var isColonCall = false;
        if (callExpr.PrefixExpr is LuaIndexExprSyntax indexExpr)
        {
            isColonCall = indexExpr.IsColonIndex;
        }

        LuaUnion.Each(prefixTy, type =>
        {
            if (type is LuaMethod luaMethod)
            {
                var args = callExpr.ArgList?.ArgList.ToList();
                if (args == null) return;
                var perfectSig = luaMethod.FindPerfectSignature(callExpr, Context);
                var isColonDefine = perfectSig.ColonDefine;
                switch ((isColonCall, isColonDefine))
                {
                    case (true, false):
                    {
                        if (perfectSig.Parameters.FirstOrDefault() is { Name: not "self" })
                        {
                            callExpr.Tree.PushDiagnostic(new Diagnostic(
                                DiagnosticSeverity.Warning,
                                DiagnosticCode.TypeNotMatch,
                                "The first parameter of the method must be 'self'",
                                callExpr.ArgList?.LeftParen?.Location ?? callExpr.Location
                            ));
                            return;
                        }

                        CheckFuncCallParams(callExpr, perfectSig.Parameters.Skip(1).ToList(), args);
                        break;
                    }
                    case (false, true):
                    {
                        var declarations = new List<Symbol.Symbol>
                            { new VirtualSymbol("self", luaMethod.SelfType) };
                        declarations.AddRange(perfectSig.Parameters);
                        CheckFuncCallParams(callExpr, declarations, args);
                        break;
                    }
                    default:
                    {
                        CheckFuncCallParams(callExpr, perfectSig.Parameters, args);
                        break;
                    }
                }
            }
        });
    }

    public override void RemoveCache(DocumentId documentId)
    {
        BindData.Remove(documentId);
    }
}
