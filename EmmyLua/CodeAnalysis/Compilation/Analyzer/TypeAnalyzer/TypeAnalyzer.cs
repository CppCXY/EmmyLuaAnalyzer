using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compile.Diagnostic;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.TypeAnalyzer;

public class TypeAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    private SearchContext Context => Compilation.SearchContext;

    public override void Analyze(DocumentId documentId)
    {
        var declarationTree = Compilation.GetSymbolTree(documentId);
        if (declarationTree is null)
        {
            return;
        }

        if (Compilation.GetSyntaxTree(documentId) is { } syntaxTree &&
            Compilation.GetSymbolTree(documentId) is { } symbolTree)
        {
            foreach (var node in syntaxTree.SyntaxRoot.Descendants)
            {
                switch (node)
                {
                    case LuaLocalStatSyntax luaLocalStat:
                    {
                        LocalTypeAnalysis(luaLocalStat, symbolTree);
                        break;
                    }
                    case LuaAssignStatSyntax luaAssignStat:
                    {
                        AssignTypeAnalysis(luaAssignStat, symbolTree);
                        break;
                    }
                    case LuaForStatSyntax luaForStat:
                    {
                        ForStatBindAnalysis(luaForStat, symbolTree);
                        break;
                    }
                    case LuaForRangeStatSyntax luaForRangeStat:
                    {
                        ForRangeBindAnalysis(luaForRangeStat, symbolTree);
                        break;
                    }
                    case LuaCallExprSyntax luaCallExpr:
                    {
                        CallExprAnalysis(luaCallExpr, symbolTree);
                        break;
                    }
                }
            }
        }
    }

    private void LocalTypeAnalysis(LuaLocalStatSyntax localStat, SymbolTree tree)
    {
        foreach (var localName in localStat.NameList)
        {
            var symbol = tree.FindSymbol(localName);
            if (symbol is LocalDeclaration { DeclarationType: LuaTypeRef ty } localDeclaration)
            {
                var exprType = localDeclaration.ExprRef?.GetType(Context) ?? Compilation.Builtin.Nil;
                if (!exprType.SubTypeOf(ty, Context))
                {
                    localStat.Tree.PushDiagnostic(new Diagnostic(
                        DiagnosticSeverity.Warning,
                        DiagnosticCode.TypeNotMatch,
                        $"Local variable '{localName.Name?.RepresentText}' declaration type is '{ty.ToDisplayString(Context)}' not match expr type '{exprType.ToDisplayString(Context)}'",
                        localName.Location
                    ));
                }
            }
        }
    }

    private void AssignTypeAnalysis(LuaAssignStatSyntax assignStat, SymbolTree tree)
    {
        foreach (var varExpr in assignStat.VarList)
        {
            var symbol = tree.FindSymbol(varExpr);
            switch (symbol)
            {
                case GlobalDeclaration { DeclarationType: { } ty } globalDeclaration:
                {
                    var exprType = globalDeclaration.ExprRef?.GetType(Context) ?? Compilation.Builtin.Nil;
                    if (!exprType.SubTypeOf(ty, Context))
                    {
                        assignStat.Tree.PushDiagnostic(new Diagnostic(
                            DiagnosticSeverity.Warning,
                            DiagnosticCode.TypeNotMatch,
                            $"Global {globalDeclaration.Name} declaration type '{ty.ToDisplayString(Context)}' not match expr type '{exprType.ToDisplayString(Context)}'",
                            varExpr.Location
                        ));
                    }

                    break;
                }
                case IndexDeclaration indexDeclaration:
                {
                    break;
                }
                case AssignSymbol assignSymbol:
                {
                    break;
                }
            }
        }
    }

    private void ForStatBindAnalysis(LuaForStatSyntax forStat, SymbolTree tree)
    {
        if (forStat.IteratorName is { } itName)
        {
            var symbol = tree.FindSymbol(itName);
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

    private void ForRangeBindAnalysis(LuaForRangeStatSyntax forRangeStat, SymbolTree tree)
    {
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

                var symbol = tree.FindSymbol(iterName);
                if (symbol is { DeclarationType: { } declTy })
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
                    if (symbol != null) symbol.DeclarationType = ty;
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
        List<ParameterDeclaration> parameters,
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

    private void CallExprAnalysis(LuaCallExprSyntax callExpr, SymbolTree tree)
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
                        var declarations = new List<ParameterDeclaration>
                            { ParameterDeclaration.SelfParameter(luaMethod.SelfType) };
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
}
