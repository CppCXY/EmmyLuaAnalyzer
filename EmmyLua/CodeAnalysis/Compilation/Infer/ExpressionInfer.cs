using System.Diagnostics;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public static class ExpressionInfer
{
    public static LuaType InferExpr(LuaExprSyntax expr, SearchContext context)
    {
        return expr switch
        {
            LuaUnaryExprSyntax unaryExpr => InferUnaryExpr(unaryExpr, context),
            LuaBinaryExprSyntax binaryExpr => InferBinaryExpr(binaryExpr, context),
            LuaCallExprSyntax callExpr => CallExprInfer.InferCallExpr(callExpr, context),
            LuaClosureExprSyntax closureExpr => InferClosureExpr(closureExpr, context),
            LuaTableExprSyntax tableExpr => InferTableExpr(tableExpr, context),
            LuaParenExprSyntax parenExpr => InferParenExpr(parenExpr, context),
            LuaIndexExprSyntax indexExpr => InferIndexExpr(indexExpr, context),
            LuaLiteralExprSyntax literalExpr => InferLiteralExpr(literalExpr, context),
            LuaNameExprSyntax nameExpr => InferNameExpr(nameExpr, context),
            _ => throw new UnreachableException()
        };
    }

    private static LuaType InferUnaryExpr(LuaUnaryExprSyntax unaryExpr, SearchContext context)
    {
        var exprTy = context.Infer(unaryExpr.Expression).UnwrapType(context);
        var opKind = TypeOperatorKindHelper.ToTypeOperatorKind(unaryExpr.Operator);
        var op = context.GetBestMatchedUnaryOperator(opKind, exprTy);

        if (op is not null)
        {
            return op.Ret;
        }

        return unaryExpr.Operator switch
        {
            OperatorKind.UnaryOperator.OpNot or OperatorKind.UnaryOperator.OpUnm
                or OperatorKind.UnaryOperator.OpBNot => Builtin.Boolean,
            OperatorKind.UnaryOperator.OpLen => Builtin.Integer,
            _ => Builtin.Unknown
        };
    }

    private static LuaType InferBinaryExpr(LuaBinaryExprSyntax binaryExpr, SearchContext context)
    {
        var op = binaryExpr.Operator;
        return binaryExpr.Operator switch
        {
            // logic
            OperatorKind.BinaryOperator.OpLe or OperatorKind.BinaryOperator.OpGt
                or OperatorKind.BinaryOperator.OpLt or OperatorKind.BinaryOperator.OpGe
                or OperatorKind.BinaryOperator.OpNe or OperatorKind.BinaryOperator.OpEq
                => Builtin.Boolean,
            // ..
            OperatorKind.BinaryOperator.OpConcat => Builtin.String,
            // math
            OperatorKind.BinaryOperator.OpAdd or OperatorKind.BinaryOperator.OpSub
                or OperatorKind.BinaryOperator.OpMul
                or OperatorKind.BinaryOperator.OpDiv or OperatorKind.BinaryOperator.OpIDiv
                or OperatorKind.BinaryOperator.OpMod
                or OperatorKind.BinaryOperator.OpPow or OperatorKind.BinaryOperator.OpBOr
                or OperatorKind.BinaryOperator.OpBAnd
                or OperatorKind.BinaryOperator.OpBXor or OperatorKind.BinaryOperator.OpShr
                or OperatorKind.BinaryOperator.OpShl
                => GuessBinaryMathType(binaryExpr, op, context),
            // 'and' 'or'
            OperatorKind.BinaryOperator.OpAnd or OperatorKind.BinaryOperator.OpOr =>
                GuessAndOrType(binaryExpr, op, context),
            _ => Builtin.Unknown
        };
    }

    private static LuaType GuessAndOrType(LuaBinaryExprSyntax binaryExpr, OperatorKind.BinaryOperator op,
        SearchContext context)
    {
        var rhs = binaryExpr.RightExpr;

        // and
        if (op is OperatorKind.BinaryOperator.OpAnd)
        {
            return context.Infer(rhs);
        }

        // or
        var lhs = binaryExpr.LeftExpr;
        var lty = context.Infer(lhs);
        return rhs != null ? lty.Union(context.Infer(rhs)) : lty;
    }

    private static LuaType GuessBinaryMathType(LuaBinaryExprSyntax binaryExpr, OperatorKind.BinaryOperator op,
        SearchContext context)
    {
        var leftTy = context.InferAndUnwrap(binaryExpr.LeftExpr);
        var rightTy = context.InferAndUnwrap(binaryExpr.RightExpr);
        var opKind = TypeOperatorKindHelper.ToTypeOperatorKind(op);
        var bop = context.GetBestMatchedBinaryOperator(opKind, leftTy, rightTy);
        if (bop is not null)
        {
            return bop.Ret;
        }
        var bop2 = context.GetBestMatchedBinaryOperator(opKind, rightTy, leftTy);
        if (bop2 is not null)
        {
            return bop2.Ret;
        }

        if (leftTy.Equals(Builtin.Integer) && rightTy.Equals(Builtin.Integer))
        {
            return Builtin.Integer;
        }

        return Builtin.Number;
    }

    private static LuaType InferClosureExpr(LuaClosureExprSyntax closureExpr, SearchContext context)
    {
        var methodType = context.Compilation.Db.QueryTypeFromId(closureExpr.UniqueId);
        return methodType ?? Builtin.Unknown;
    }

    private static LuaType InferTableExpr(LuaTableExprSyntax tableExpr, SearchContext context)
    {
        return new LuaTableLiteralType(tableExpr);
    }

    private static LuaType InferParenExpr(LuaParenExprSyntax parenExpr, SearchContext context)
    {
        return context.Infer(parenExpr.Inner);
    }

    private static LuaType InferIndexExpr(LuaIndexExprSyntax indexExpr, SearchContext context)
    {
        var declaration = context.FindDeclaration(indexExpr);
        return declaration?.Type ?? Builtin.Unknown;
    }

    private static LuaType InferLiteralExpr(LuaLiteralExprSyntax literalExpr, SearchContext context)
    {
        return literalExpr.Literal switch
        {
            LuaIntegerToken => Builtin.Integer,
            LuaFloatToken or LuaNumberToken => Builtin.Number,
            LuaStringToken => Builtin.String,
            LuaNilToken => Builtin.Nil,
            LuaBoolToken => Builtin.Boolean,
            _ => Builtin.Unknown
        };
    }

    private static LuaType InferNameExpr(LuaNameExprSyntax nameExpr, SearchContext context)
    {
        if (nameExpr.Name is null)
        {
            return Builtin.Unknown;
        }

        var nameDecl = context.FindDeclaration(nameExpr);
        if (nameDecl?.Type is { } ty)
        {
            return ty;
        }

        if (nameExpr.Name is { Text: "self"})
        {
            return InferSelf(nameExpr, context);
        }

        // TODO infer from Env

        return Builtin.Unknown;
    }

    private static LuaType InferSelf(LuaNameExprSyntax selfExpr, SearchContext context)
    {
        var closures = selfExpr.Ancestors.OfType<LuaClosureExprSyntax>();
        foreach (var closure in closures)
        {
            var stat = closure.Parent;
            if (stat is LuaFuncStatSyntax { IndexExpr.PrefixExpr: { } expr })
            {
                return context.Infer(expr);
            }
        }

        return Builtin.Unknown;
    }
}
