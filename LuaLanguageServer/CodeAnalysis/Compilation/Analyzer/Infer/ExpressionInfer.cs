using System.Diagnostics;

using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;

public static class ExpressionInfer
{
    public static ILuaType InferExpr(LuaExprSyntax expr, SearchContext context)
    {
        return expr switch
        {
            LuaUnaryExprSyntax unaryExpr => InferUnaryExpr(unaryExpr, context),
            LuaBinaryExprSyntax binaryExpr => InferBinaryExpr(binaryExpr, context),
            LuaCallExprSyntax callExpr => context.CallExprInfer.InferCallExpr(callExpr, context),
            LuaClosureExprSyntax closureExpr => InferClosureExpr(closureExpr, context),
            LuaTableExprSyntax tableExpr => InferTableExpr(tableExpr, context),
            LuaParenExprSyntax parenExpr => InferParenExpr(parenExpr, context),
            LuaIndexExprSyntax indexExpr => InferIndexExpr(indexExpr, context),
            LuaLiteralExprSyntax literalExpr => InferLiteralExpr(literalExpr, context),
            LuaNameExprSyntax nameExpr => InferNameExpr(nameExpr, context),
            _ => throw new UnreachableException()
        };
    }

    private static ILuaType InferUnaryExpr(LuaUnaryExprSyntax unaryExpr, SearchContext context)
    {
        return unaryExpr.Operator switch
        {
            OperatorKind.UnaryOperator.OpNot or OperatorKind.UnaryOperator.OpUnm
                or OperatorKind.UnaryOperator.OpBNot => context.Compilation.Builtin.Boolean,
            OperatorKind.UnaryOperator.OpLen => context.Compilation.Builtin.Number,
            _ => context.Compilation.Builtin.Unknown
        };
    }

    private static ILuaType InferBinaryExpr(LuaBinaryExprSyntax binaryExpr, SearchContext context)
    {
        var op = binaryExpr.Operator;
        return op switch
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

    private static ILuaType GuessAndOrType(LuaBinaryExprSyntax binaryExpr, OperatorKind.BinaryOperator op,
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
        var symbol = context.Infer(lhs);
        return rhs != null ? LuaUnion.UnionType(symbol, context.Infer(rhs)) : symbol;
    }

    private static ILuaType GuessBinaryMathType(LuaBinaryExprSyntax binaryExpr, OperatorKind.BinaryOperator op,
        SearchContext context)
    {
        var lhs = binaryExpr.LeftExpr;
        // var rhs = binaryExpr.RightExpr;
        var lhsTy = context.Infer(lhs);
        // var rhsSymbol = context.Infer(rhs);
        // TODO: for override
        return lhsTy;
    }

    private static ILuaType InferClosureExpr(LuaClosureExprSyntax closureExpr, SearchContext context)
    {
        // TODO: infer func type
        return context.Compilation.Builtin.Unknown;
    }

    private static ILuaType InferTableExpr(LuaTableExprSyntax tableExpr, SearchContext context)
    {
        ILuaType keyType = context.Compilation.Builtin.Unknown;
        ILuaType elementType = context.Compilation.Builtin.Unknown;
        foreach (var field in tableExpr.FieldList)
        {
            if (field.IsValue)
            {
                elementType = LuaUnion.UnionType(elementType, context.Infer(field.Value));
            }
            else
            {
                if (field.IsNameKey || field.IsStringKey)
                {
                    keyType = LuaUnion.UnionType(keyType, context.Compilation.Builtin.String);
                }
                else if(field.IsNumberKey)
                {
                    keyType = LuaUnion.UnionType(keyType, context.Compilation.Builtin.Number);
                }
                else
                {
                    keyType = LuaUnion.UnionType(keyType, context.Infer(field.ExprKey));
                }

                elementType = LuaUnion.UnionType(elementType, context.Infer(field.Value));
            }
        }

        switch ((keyType, elementType))
        {
            case (Unknown, Unknown):
            {
                return new PrimitiveGenericTable(context.Compilation.Builtin.Unknown, context.Compilation.Builtin.Unknown);
            }
            case (Unknown, _):
            {
                return new Type.LuaArray(elementType);
            }
            default:
            {
                return new PrimitiveGenericTable(keyType, elementType);
            }
        }
    }

    private static ILuaType InferParenExpr(LuaParenExprSyntax parenExpr, SearchContext context)
    {
        return context.Infer(parenExpr.Inner);
    }

    private static ILuaType InferIndexExpr(LuaIndexExprSyntax indexExpr, SearchContext context)
    {
        if (indexExpr.PrefixExpr is { } prefixExpr)
        {
            var key = IndexKey.FromIndexExpr(indexExpr, context);
            var prefixTy = InferExpr(prefixExpr, context);
            var ty = prefixTy.IndexMember(key, context).FirstOrDefault()?.GetType(context);
            if (ty is not null)
            {
                return ty;
            }
        }

        return context.Compilation.Builtin.Unknown;
    }

    private static ILuaType InferLiteralExpr(LuaLiteralExprSyntax literalExpr, SearchContext context)
    {
        return literalExpr.Literal switch
        {
            LuaIntegerToken => context.Compilation.Builtin.Integer,
            LuaStringToken => context.Compilation.Builtin.String,
            LuaNilToken => context.Compilation.Builtin.Nil,
            LuaBoolToken => context.Compilation.Builtin.Boolean,
            _ => context.Compilation.Builtin.Unknown
        };
    }

    private static ILuaType InferNameExpr(LuaNameExprSyntax nameExpr, SearchContext context)
    {
        // var declarationTree = context.Compilation.GetDeclarationTree(nameExpr.Tree);
        // var declaration = declarationTree.FindDeclaration(nameExpr)?.FirstDeclaration.SyntaxElement;
        // if (declaration is not null && !ReferenceEquals(declaration, nameExpr))
        // {
        //     return context.Infer(declaration);
        // }
        // else
        // {
        //     // TODO 找到他的表达式对象
        // }
        return context.Compilation.Builtin.Unknown;
    }
}
