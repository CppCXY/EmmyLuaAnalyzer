using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Infer;

public static class ExpressionInfer
{
    public static ILuaSymbol InferExpr(LuaExprSyntax expr, SearchContext context)
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

    private static ILuaSymbol InferExprInner(LuaExprSyntax expr, SearchContext context)
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

    private static ILuaSymbol InferUnaryExpr(LuaUnaryExprSyntax unaryExpr, SearchContext context)
    {
        return unaryExpr.Operator switch
        {
            OperatorKind.UnaryOperator.OpNot or OperatorKind.UnaryOperator.OpUnm
                or OperatorKind.UnaryOperator.OpBNot => context.Compilation.Builtin.Boolean,
            OperatorKind.UnaryOperator.OpLen => context.Compilation.Builtin.Number,
            _ => context.Compilation.Builtin.Unknown
        };
    }

    private static ILuaSymbol InferBinaryExpr(LuaBinaryExprSyntax binaryExpr, SearchContext context)
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

    private static ILuaSymbol GuessAndOrType(LuaBinaryExprSyntax binaryExpr, OperatorKind.BinaryOperator op,
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
        return rhs != null ? UnionSymbol.Union(symbol, context.Infer(rhs)) : symbol;
    }

    private static ILuaSymbol GuessBinaryMathType(LuaBinaryExprSyntax binaryExpr, OperatorKind.BinaryOperator op,
        SearchContext context)
    {
        var lhs = binaryExpr.LeftExpr;
        // var rhs = binaryExpr.RightExpr;
        var lhsSymbol = context.Infer(lhs);
        // var rhsSymbol = context.Infer(rhs);
        // TODO: for override
        return lhsSymbol;
    }

    private static ILuaSymbol InferCallExpr(LuaCallExprSyntax callExpr, SearchContext context)
    {
        ILuaSymbol ret = context.Compilation.Builtin.Unknown;
        var prefixExpr = callExpr.PrefixExpr;
        var symbol = context.Infer(prefixExpr);
        UnionSymbol.Each(symbol, s =>
        {
            switch (s)
            {
                case FuncSymbol func:
                {
                    var args = callExpr.ArgList?.ArgList;
                    if (args == null) return;
                    var argSymbols = args.Select(context.Infer);
                    var perfectSig = func.FindPerfectSignature(argSymbols, context);
                    if (perfectSig.ReturnType is { } retTy)
                    {
                        ret = UnionSymbol.Union(ret, retTy);
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

    private static ILuaSymbol InferClosureExpr(LuaClosureExprSyntax closureExpr, SearchContext context)
    {
        // TODO: infer table type
        return context.Compilation.Builtin.Unknown;
    }

    private static ILuaSymbol InferTableExpr(LuaTableExprSyntax tableExpr, SearchContext context)
    {
        ILuaSymbol keyType = context.Compilation.Builtin.Unknown;
        ILuaSymbol elementType = context.Compilation.Builtin.Unknown;
        foreach (var field in tableExpr.FieldList)
        {
            if (field.IsValue)
            {
                elementType = UnionSymbol.Union(elementType, context.Infer(field.Value));
            }
            else
            {
                if (field.IsNameKey || field.IsStringKey)
                {
                    keyType = UnionSymbol.Union(keyType, context.Compilation.Builtin.String);
                }
                else if(field.IsNumberKey)
                {
                    keyType = UnionSymbol.Union(keyType, context.Compilation.Builtin.Number);
                }
                else
                {
                    keyType = UnionSymbol.Union(keyType, context.Infer(field.ExprKey));
                }

                elementType = UnionSymbol.Union(elementType, context.Infer(field.Value));
            }
        }

        switch ((keyType, elementType))
        {
            // TODO create for empty table
            case (UnknownSymbol, UnknownSymbol):
            {
                return context.Compilation.Builtin.Unknown;
            }
            // TODO create for array table
            case (UnknownSymbol, _):
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

    private static ILuaSymbol InferParenExpr(LuaParenExprSyntax parenExpr, SearchContext context)
    {
        return context.Infer(parenExpr.Inner);
    }

    private static ILuaSymbol InferIndexExpr(LuaIndexExprSyntax indexExpr, SearchContext context)
    {
        // TODO: infer index type
        return context.Compilation.Builtin.Unknown;
    }

    private static ILuaSymbol InferLiteralExpr(LuaLiteralExprSyntax literalExpr, SearchContext context)
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

    private static ILuaSymbol InferNameExpr(LuaNameExprSyntax nameExpr, SearchContext context)
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

    private static ILuaSymbol InferRequireExpr(LuaRequireExprSyntax requireExpr, SearchContext context)
    {
        // var path = requireExpr.ModulePath;
        // var source = context.Compilation.Resolve.ModelPath(path);
        // return context.Infer(source);
        throw new NotImplementedException();
    }
}
