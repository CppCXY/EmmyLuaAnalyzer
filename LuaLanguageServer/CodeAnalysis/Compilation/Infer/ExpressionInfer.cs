using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Infer;

public static class ExpressionInfer
{
    public static ILuaType InferExpr(LuaExprSyntax expr, SearchContext context)
    {
        if (expr is LuaIndexExprSyntax or LuaNameExprSyntax)
        {
            var declarationTree = context.Compilation.GetDeclarationTree(expr.Tree);
            var declaration = declarationTree.Find(expr)?.FirstDeclaration.SyntaxElement;
            if (declaration is not null && !ReferenceEquals(declaration, expr))
            {
                return context.Infer(declaration);
            }
        }

        return InferExprInner(expr, context);
    }

    private static ILuaType InferExprInner(LuaExprSyntax expr, SearchContext context)
    {
        return expr switch
        {
            LuaUnaryExprSyntax unaryExpr => InferUnaryExpr(unaryExpr, context),
            LuaBinaryExprSyntax binaryExpr => InferBinaryExpr(binaryExpr, context),
            LuaCallExprSyntax callExpr => InferCallExpr(callExpr, context),
            LuaClosureExprSyntax closureExpr => InferClosureExpr(closureExpr, context),
            LuaTableExprSyntax tableExpr => InferTableExpr(tableExpr, context),
            LuaParenExprSyntax parenExpr => InferParenExpr(parenExpr, context),
            LuaIndexExprSyntax indexExpr => InferIndexExpr(indexExpr, context),
            LuaLiteralExprSyntax literalExpr => InferLiteralExpr(literalExpr, context),
            LuaNameExprSyntax nameExpr => InferNameExpr(nameExpr, context),
            LuaRequireExprSyntax requireExpr => InferRequireExpr(requireExpr, context),
            _ => throw new NotImplementedException()
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
        return rhs != null ? Union.UnionType(symbol, context.Infer(rhs)) : symbol;
    }

    private static ILuaType GuessBinaryMathType(LuaBinaryExprSyntax binaryExpr, OperatorKind.BinaryOperator op,
        SearchContext context)
    {
        var lhs = binaryExpr.LeftExpr;
        // var rhs = binaryExpr.RightExpr;
        var lhsSymbol = context.Infer(lhs);
        // var rhsSymbol = context.Infer(rhs);
        // TODO: for override
        return lhsSymbol;
    }

    private static ILuaType InferCallExpr(LuaCallExprSyntax callExpr, SearchContext context)
    {
        ILuaType ret = context.Compilation.Builtin.Unknown;
        var prefixExpr = callExpr.PrefixExpr;
        var symbol = context.Infer(prefixExpr);
        Union.Each(symbol, s =>
        {
            switch (s)
            {
                case Func func:
                {
                    var args = callExpr.ArgList?.ArgList;
                    if (args == null) return;
                    var argSymbols = args.Select(context.Infer);
                    var perfectSig = func.FindPerfectSignature(argSymbols, context);
                    if (perfectSig.ReturnType is { } retTy)
                    {
                        ret = Union.UnionType(ret, retTy);
                    }

                    break;
                }
            }
        });

        // TODO class.new return self
        // if (prefixExpr is LuaIndexExprSyntax indexExpr)
        // {
        //     var fnName = indexExpr.Name?.RepresentText;
        //     if (fnName is not null)
        //     {
        //         var fnSymbol = context.Compilation.GetSymbol(fnName);
        //     }
        // }

        return ret;
    }

    private static ILuaType InferClosureExpr(LuaClosureExprSyntax closureExpr, SearchContext context)
    {
        // TODO: infer table type
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
                elementType = Union.UnionType(elementType, context.Infer(field.Value));
            }
            else
            {
                if (field.IsNameKey || field.IsStringKey)
                {
                    keyType = Union.UnionType(keyType, context.Compilation.Builtin.String);
                }
                else if(field.IsNumberKey)
                {
                    keyType = Union.UnionType(keyType, context.Compilation.Builtin.Number);
                }
                else
                {
                    keyType = Union.UnionType(keyType, context.Infer(field.ExprKey));
                }

                elementType = Union.UnionType(elementType, context.Infer(field.Value));
            }
        }

        switch ((keyType, elementType))
        {
            // TODO create for empty table
            case (Unknown, Unknown):
            {
                return context.Compilation.Builtin.Unknown;
            }
            // TODO create for array table
            case (Unknown, _):
            {
                return context.Compilation.Builtin.Unknown;
            }
            // TODO create for table<key, value>
            default:
            {
                return context.Compilation.Builtin.Unknown;
            }
        }
    }

    private static ILuaType InferParenExpr(LuaParenExprSyntax parenExpr, SearchContext context)
    {
        return context.Infer(parenExpr.Inner);
    }

    private static ILuaType InferIndexExpr(LuaIndexExprSyntax indexExpr, SearchContext context)
    {
        // TODO: infer index type
        return context.Compilation.Builtin.Unknown;
    }

    private static ILuaType InferLiteralExpr(LuaLiteralExprSyntax literalExpr, SearchContext context)
    {
        return literalExpr.Literal.Kind switch
        {
            LuaTokenKind.TkNumber => context.Compilation.Builtin.Number,
            LuaTokenKind.TkString or LuaTokenKind.TkLongString => context.Compilation.Builtin.String,
            LuaTokenKind.TkNil => context.Compilation.Builtin.Nil,
            LuaTokenKind.TkTrue or LuaTokenKind.TkFalse => context.Compilation.Builtin.Boolean,
            _ => context.Compilation.Builtin.Unknown
        };
    }

    private static ILuaType InferNameExpr(LuaNameExprSyntax nameExpr, SearchContext context)
    {
        // var name = nameExpr.Name;
        // if (nameExpr.Prev is not null)
        // {
        //     var prevSymbol = InferExpr(nameExpr.Prev, context);
        //     if (prevSymbol is not LuaTableSymbol tableSymbol)
        //     {
        //         return context.Compilation.Builtin.Unknown;
        //     }
        //
        //     if (tableSymbol.TryGetMember(name, out var member))
        //     {
        //         return member;
        //     }
        //
        //     return context.Compilation.Builtin.Unknown;
        // }
        //
        // if (nameExpr.Name is LuaIdentifierNameSyntax identifierName)
        // {
        //     var symbol = context.Compilation.GetSymbol(identifierName);
        //     if (symbol is not null)
        //     {
        //         return symbol;
        //     }
        // }

        return context.Compilation.Builtin.Unknown;
    }

    private static ILuaType InferRequireExpr(LuaRequireExprSyntax requireExpr, SearchContext context)
    {
        // var path = requireExpr.ModulePath;
        // var source = context.Compilation.Resolve.ModelPath(path);
        // return context.Infer(source);
        throw new NotImplementedException();
    }
}
