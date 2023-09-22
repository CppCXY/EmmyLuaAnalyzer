using LuaLanguageServer.CodeAnalysis.Compile.Parser;
using LuaLanguageServer.CodeAnalysis.Kind;

namespace LuaLanguageServer.CodeAnalysis.Compile.Grammar.Lua;

public static class ExpressionParser
{
    public static CompleteMarker Expression(LuaParser p)
    {
        return SubExpression(p, 0);
    }

    private static CompleteMarker SubExpression(LuaParser p, int limit)
    {
        var m = p.Marker();
        CompleteMarker cm;
        try
        {
            var uop = OperatorKind.ToUnaryOperator(p.Current);
            if (uop != OperatorKind.UnaryOperator.OpNop)
            {
                p.Bump();
                cm = SubExpression(p, OperatorKind.UNARY_PRIORITY);
                if (!cm.IsComplete)
                {
                    return m.Fail(p, LuaSyntaxKind.UnaryExpr, "unary operator not followed by expression");
                }
            }
            else
            {
                cm = SimpleExpression(p);
            }

            var op = OperatorKind.ToBinaryOperator(p.Current);
            while (op != OperatorKind.BinaryOperator.OpNop && OperatorKind.Priority[(int)op].Left > limit)
            {
                m = cm.Precede(p);
                p.Bump();
                if (!SubExpression(p, OperatorKind.Priority[(int)op].Right).IsComplete)
                {
                    return m.Fail(p, LuaSyntaxKind.BinaryExpr, "binary operator not followed by expression");
                }

                cm = m.Complete(p, LuaSyntaxKind.BinaryExpr);
                op = OperatorKind.ToBinaryOperator(p.Current);
            }

            return cm;
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.BinaryExpr, e.Message);
        }
    }

    private static CompleteMarker SimpleExpression(LuaParser p)
    {
        switch (p.Current)
        {
            case LuaTokenKind.TkNumber:
            case LuaTokenKind.TkInt:
            case LuaTokenKind.TkNil:
            case LuaTokenKind.TkTrue:
            case LuaTokenKind.TkFalse:
            case LuaTokenKind.TkDots:
            case LuaTokenKind.TkString:
            case LuaTokenKind.TkLongString:
            {
                var m = p.Marker();
                p.Bump();
                return m.Complete(p, LuaSyntaxKind.LiteralExpr);
            }
            case LuaTokenKind.TkLeftBrace:
            {
                return TableConstructor(p);
            }
            case LuaTokenKind.TkFunction:
            {
                return ClosureExpr(p);
            }
            default:
            {
                return SuffixExpression(p);
            }
        }
    }

    private static CompleteMarker ClosureExpr(LuaParser p)
    {
        var m = p.Marker();
        p.Bump();
        try
        {
            StatementParser.FunctionBody(p);

            return m.Complete(p, LuaSyntaxKind.ClosureExpr);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.ClosureExpr, e.Message);
        }
    }

    private static CompleteMarker TableConstructor(LuaParser p)
    {
        var m = p.Marker();
        p.Bump();
        try
        {
            if (p.Current is LuaTokenKind.TkRightBrace)
            {
                p.Bump();
                return m.Complete(p, LuaSyntaxKind.TableExpr);
            }

            FieldList(p);
            p.Expect(LuaTokenKind.TkRightBrace);

            return m.Complete(p, LuaSyntaxKind.TableExpr);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.TableExpr, e.Message);
        }
    }

    private static void FieldList(LuaParser p)
    {
        var cm = Field(p);
        while (cm.IsComplete && p.Current is LuaTokenKind.TkComma or LuaTokenKind.TkSemicolon)
        {
            p.Bump();
            if (p.Current is LuaTokenKind.TkRightBrace)
            {
                break;
            }

            cm = Field(p);
        }
    }

    private static CompleteMarker Field(LuaParser p)
    {
        var m = p.Marker();
        try
        {
            switch (p.Current)
            {
                case LuaTokenKind.TkLeftBracket:
                {
                    p.Bump();
                    if (p.Current is LuaTokenKind.TkString or LuaTokenKind.TkNumber)
                    {
                        p.Bump();
                    }
                    else
                    {
                        Expression(p);
                    }
                    p.Expect(LuaTokenKind.TkRightBracket);
                    p.Expect(LuaTokenKind.TkAssign);
                    Expression(p);
                    return m.Complete(p, LuaSyntaxKind.TableFieldAssign);
                }
                case LuaTokenKind.TkName:
                {
                    if (p.LookAhead is LuaTokenKind.TkAssign)
                    {
                        p.Bump();
                        p.Bump();
                        Expression(p);
                        return m.Complete(p, LuaSyntaxKind.TableFieldAssign);
                    }

                    return FieldExpr(p);
                }
                default:
                {
                    return FieldExpr(p);
                }
            }
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.TableFieldAssign, e.Message);
        }
    }

    private static CompleteMarker FieldExpr(LuaParser p)
    {
        var m = p.Marker();
        try
        {
            Expression(p);
            return m.Complete(p, LuaSyntaxKind.TableFieldValue);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.TableFieldValue, e.Message);
        }
    }

    public static CompleteMarker PrimaryExpression(LuaParser p)
    {
        var m = p.Marker();
        try
        {
            switch (p.Current)
            {
                case LuaTokenKind.TkName:
                {
                    p.Bump();
                    return m.Complete(p, LuaSyntaxKind.NameExpr);
                }
                case LuaTokenKind.TkLeftParen:
                {
                    p.Bump();
                    Expression(p);
                    p.Expect(LuaTokenKind.TkRightParen);
                    return m.Complete(p, LuaSyntaxKind.ParenExpr);
                }
                default:
                {
                    throw new UnexpectedTokenException($"unexpected symbol {p.Current}", p.Current);
                }
            }
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.NameExpr, e.Message);
        }
    }

    public static CompleteMarker IndexExpr(LuaParser p)
    {
        var m = p.Marker();
        try
        {
            switch (p.Current)
            {
                case LuaTokenKind.TkLeftBracket:
                {
                    p.Bump();
                    Expression(p);
                    p.Expect(LuaTokenKind.TkRightBracket);
                    break;
                }
                case LuaTokenKind.TkDot:
                {
                    p.Bump();
                    if (p.Current is not LuaTokenKind.TkName)
                    {
                        return m.Fail(p, LuaSyntaxKind.IndexExpr, "expected a name after '.'");
                    }

                    p.Bump();
                    break;
                }
                case LuaTokenKind.TkColon:
                {
                    p.Bump();
                    if (p.Current is not LuaTokenKind.TkName)
                    {
                        return m.Fail(p, LuaSyntaxKind.IndexExpr, "expected a name after ':'");
                    }

                    p.Bump();
                    break;
                }
                default:
                {
                    throw new UnexpectedTokenException($"unexpected symbol {p.Current}", p.Current);
                }
            }

            return m.Complete(p, LuaSyntaxKind.IndexExpr);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.IndexExpr, e.Message);
        }
    }

    public static CompleteMarker SuffixExpression(LuaParser p)
    {
        var m = p.Marker();
        var cm = PrimaryExpression(p);
        var suffix = false;
        while (true)
        {
            switch (p.Current)
            {
                case LuaTokenKind.TkDot or LuaTokenKind.TkColon or LuaTokenKind.TkLeftBracket:
                {
                    IndexExpr(p);
                    break;
                }
                case LuaTokenKind.TkString or LuaTokenKind.TkLongString or LuaTokenKind.TkLeftParen
                    or LuaTokenKind.TkLeftBrace:
                {
                    CallExpr(p);
                    break;
                }
                default:
                {
                    goto endLoop;
                }
            }

            suffix = true;
        }

        endLoop:
        return suffix ? m.Complete(p, LuaSyntaxKind.SuffixExpr) : cm;
    }

    private static CompleteMarker CallExpr(LuaParser p)
    {
        var m = p.Marker();
        try
        {
            switch (p.Current)
            {
                case LuaTokenKind.TkString or LuaTokenKind.TkLongString:
                {
                    p.Bump();
                    break;
                }
                case LuaTokenKind.TkLeftParen:
                {
                    p.Bump();
                    if (p.Current is not LuaTokenKind.TkRightParen)
                    {
                        Expression(p);
                        while (p.Current is LuaTokenKind.TkComma)
                        {
                            p.Bump();
                            Expression(p);
                        }
                    }

                    p.Expect(LuaTokenKind.TkRightParen);
                    break;
                }
                case LuaTokenKind.TkLeftBrace:
                {
                    TableConstructor(p);
                    break;
                }
                default:
                {
                    throw new UnexpectedTokenException($"unexpected symbol {p.Current}", p.Current);
                }
            }

            return m.Complete(p, LuaSyntaxKind.CallExpr);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.CallExpr, e.Message);
        }
    }

    public static CompleteMarker VarDefinition(LuaParser p)
    {
        var cm = SuffixExpression(p);
        var m = cm.Precede(p);
        return !cm.IsComplete ? m.Fail(p, LuaSyntaxKind.VarDef, "expected a variable name") : m.Complete(p, LuaSyntaxKind.VarDef);
    }
}
