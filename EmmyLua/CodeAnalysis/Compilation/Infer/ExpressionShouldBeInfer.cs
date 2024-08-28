using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public static class ExpressionShouldBeInfer
{
    public static LuaType InferExprShouldBe(LuaExprSyntax expr, SearchContext context)
    {
        return expr.Parent switch
        {
            LuaCallArgListSyntax callArgList => InferCallArgList(callArgList, expr, context),
            LuaTableFieldSyntax tableField => InferTableField(tableField, context),
            _ => Builtin.Unknown
        };
    }

    private static LuaType InferCallArgList(LuaCallArgListSyntax callArgList, LuaExprSyntax expr, SearchContext context)
    {
        if (callArgList.Parent is not LuaCallExprSyntax callExpr)
        {
            return Builtin.Unknown;
        }

        var activeParam = callArgList.ChildTokens(LuaTokenKind.TkComma)
            .Count(comma => comma.Position <= expr.Position);

        var prefixType = context.Infer(callExpr.PrefixExpr);
        LuaType exprType = Builtin.Unknown;
        foreach(var methodType in context.FindCallableType(prefixType))
        {
            var colonDefine = methodType.ColonDefine;
            var colonCall = (callExpr.PrefixExpr as LuaIndexExprSyntax)?.IsColonIndex ?? false;
            switch ((colonDefine, colonCall))
            {
                case (true, false):
                {
                    activeParam--;
                    break;
                }
                case (false, true):
                {
                    activeParam++;
                    break;
                }
            }

            if (activeParam >= 0 && activeParam < methodType.MainSignature.Parameters.Count)
            {
                var param = methodType.MainSignature.Parameters[activeParam];
                exprType = exprType.Union(param.Type, context);
            }
        }

        return exprType;
    }

    private static LuaType InferTableField(LuaTableFieldSyntax tableField, SearchContext context)
    {
        var tableExpr = tableField.ParentTable;
        if (tableExpr is null)
        {
            return Builtin.Unknown;
        }

        var exprShouldType = InferExprShouldBe(tableExpr, context);
        if (tableField.Name is { } name)
        {
            return context.FindMember(exprShouldType, name)?.Type ?? Builtin.Unknown;
        }

        return Builtin.Unknown;
    }
}
