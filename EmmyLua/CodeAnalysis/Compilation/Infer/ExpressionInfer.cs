using System.Diagnostics;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Kind;
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
        var exprTy = context.Infer(unaryExpr.Expression);
        var opKind = TypeOperatorKindHelper.ToTypeOperatorKind(unaryExpr.Operator);
        var op = context.Compilation.ProjectIndex.TypeIndex.GetBestMatchedUnaryOperator(opKind, exprTy);

        if (op is not null)
        {
            return op.Ret;
        }

        return unaryExpr.Operator switch
        {
            OperatorKind.UnaryOperator.OpNot or OperatorKind.UnaryOperator.OpUnm
                or OperatorKind.UnaryOperator.OpBNot => context.Compilation.Builtin.Boolean,
            OperatorKind.UnaryOperator.OpLen => context.Compilation.Builtin.Number,
            _ => context.Compilation.Builtin.Unknown
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
                => context.Compilation.Builtin.Boolean,
            // ..
            OperatorKind.BinaryOperator.OpConcat => context.Compilation.Builtin.String,
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
            _ => context.Compilation.Builtin.Unknown
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
        var leftTy = context.Infer(binaryExpr.LeftExpr);
        var rightTy = context.Infer(binaryExpr.RightExpr);
        var opKind = TypeOperatorKindHelper.ToTypeOperatorKind(op);
        var bop = context.Compilation.ProjectIndex.TypeIndex.GetBestMatchedBinaryOperator(opKind, leftTy, rightTy);
        if (bop is not null)
        {
            return bop.Ret;
        }
        return leftTy;
    }

    private static LuaType InferClosureExpr(LuaClosureExprSyntax closureExpr, SearchContext context)
    {
        // var ty = context.Compilation.ProjectIndex.NameDeclaration
        throw new NotImplementedException();
    }

    private static LuaType InferTableExpr(LuaTableExprSyntax tableExpr, SearchContext context)
    {
        return new LuaTableLiteralType(tableExpr.UniqueId);
    }

    private static LuaType InferParenExpr(LuaParenExprSyntax parenExpr, SearchContext context)
    {
        return context.Infer(parenExpr.Inner);
    }

    private static LuaType InferIndexExpr(LuaIndexExprSyntax indexExpr, SearchContext context)
    {
        var declaration = DeclarationInfer.GetSymbolTree(indexExpr, context)?.FindDeclaration(indexExpr, context);

        if (declaration is { DeclarationType: { } ty2 })
        {
            return ty2;
        }

        return context.Compilation.Builtin.Unknown;
    }

    private static LuaType InferLiteralExpr(LuaLiteralExprSyntax literalExpr, SearchContext context)
    {
        return literalExpr.Literal switch
        {
            LuaIntegerToken => context.Compilation.Builtin.Integer,
            LuaFloatToken or LuaNumberToken => context.Compilation.Builtin.Number,
            LuaStringToken => context.Compilation.Builtin.String,
            LuaNilToken => context.Compilation.Builtin.Nil,
            LuaBoolToken => context.Compilation.Builtin.Boolean,
            _ => context.Compilation.Builtin.Unknown
        };
    }

    private static LuaType InferNameExpr(LuaNameExprSyntax nameExpr, SearchContext context)
    {
        if (nameExpr.Name is null)
        {
            return context.Compilation.Builtin.Unknown;
        }

        var symbolTree = DeclarationInfer.GetSymbolTree(nameExpr, context);
        var nameDecl = symbolTree?.FindDeclaration(nameExpr, context);

        if (nameDecl?.DeclarationType is { } ty)
        {
            return ty;
        }

        return context.Compilation.Builtin.Unknown;
    }
}
