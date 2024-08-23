﻿using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Compile.Parser;

namespace EmmyLua.CodeAnalysis.Compile.Grammar.Lua;

public static class ExpressionParser
{
    public static CompleteMarker Expression(LuaParser p)
    {
        return SubExpression(p, 0);
    }

    private static CompleteMarker SubExpression(LuaParser p, int limit)
    {
        var m = p.Marker();
        try
        {
            var uop = OperatorKind.ToUnaryOperator(p.Current);
            CompleteMarker cm;
            if (uop != OperatorKind.UnaryOperator.OpNop)
            {
                p.Bump();
                if (!SubExpression(p, OperatorKind.UNARY_PRIORITY).IsComplete)
                {
                    return m.Fail(p, LuaSyntaxKind.UnaryExpr, "unary operator not followed by expression");
                }
                cm = m.Complete(p, LuaSyntaxKind.UnaryExpr);
                m = cm.Precede(p);
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
            case LuaTokenKind.TkInt:
            case LuaTokenKind.TkFloat:
            case LuaTokenKind.TkComplex:
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

    public static CompleteMarker ParamDef(LuaParser p, bool allowDots = true)
    {
        var m = p.Marker();
        try
        {
            if (allowDots)
            {
                if (p.Current is LuaTokenKind.TkName or LuaTokenKind.TkDots)
                {
                    p.Bump();
                }
                else
                {
                    throw new UnexpectedTokenException("expected name or '...'");
                }
            }
            else
            {
                p.Expect(LuaTokenKind.TkName);
            }

            return m.Complete(p, LuaSyntaxKind.ParamName);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.ParamName, e.Message);
        }
    }

    private static CompleteMarker ParamList(LuaParser p)
    {
        var m = p.Marker();
        try
        {
            p.Expect(LuaTokenKind.TkLeftParen);
            if (p.Current is not LuaTokenKind.TkRightParen)
            {
                var cm = ParamDef(p);
                while (cm.IsComplete && p.Current is LuaTokenKind.TkComma)
                {
                    p.Bump();
                    cm = ParamDef(p);
                }
            }

            p.Expect(LuaTokenKind.TkRightParen);
            return m.Complete(p, LuaSyntaxKind.ParamList);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.ParamList, e.Message);
        }
    }


    public static CompleteMarker ClosureExpr(LuaParser p)
    {
        var m = p.Marker();
        try
        {
            p.Accept(LuaTokenKind.TkFunction);

            if (ParamList(p).IsComplete)
            {
                if (p.Current is not LuaTokenKind.TkEnd)
                {
                    BlockParser.Block(p);
                }

                p.Expect(LuaTokenKind.TkEnd);
            }

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
                    if ((p.Current is LuaTokenKind.TkString or LuaTokenKind.TkInt or LuaTokenKind.TkFloat
                        or LuaTokenKind.TkComplex) && p.LookAhead is LuaTokenKind.TkRightBracket)
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

    public static void IndexStruct(LuaParser p)
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
                    throw new UnexpectedTokenException("expected a name after '.'");
                }

                p.Bump();
                break;
            }
            case LuaTokenKind.TkColon:
            {
                p.Bump();
                if (p.Current is not LuaTokenKind.TkName)
                {
                    throw new UnexpectedTokenException("expected a name after ':'");
                }

                p.Bump();
                break;
            }
            default:
            {
                throw new UnexpectedTokenException($"unexpected symbol {p.Current}", p.Current);
            }
        }
    }

    public static CompleteMarker SuffixExpression(LuaParser p)
    {
        var cm = PrimaryExpression(p);
        while (true)
        {
            switch (p.Current)
            {
                case LuaTokenKind.TkDot or LuaTokenKind.TkColon or LuaTokenKind.TkLeftBracket:
                {
                    var m1 = cm.Precede(p);
                    try
                    {
                        IndexStruct(p);
                        cm = m1.Complete(p, LuaSyntaxKind.IndexExpr);
                    }
                    catch (UnexpectedTokenException e)
                    {
                        cm = m1.Fail(p, LuaSyntaxKind.IndexExpr, e.Message);
                    }

                    break;
                }
                case LuaTokenKind.TkString or LuaTokenKind.TkLongString or LuaTokenKind.TkLeftParen
                    or LuaTokenKind.TkLeftBrace:
                {
                    var m1 = cm.Precede(p);
                    CallArgList(p);
                    cm = m1.Complete(p, LuaSyntaxKind.CallExpr);
                    break;
                }
                default:
                {
                    goto endLoop;
                }
            }
        }

        endLoop:
        return cm;
    }

    private static CompleteMarker CallArgList(LuaParser p)
    {
        var m = p.Marker();
        try
        {
            switch (p.Current)
            {
                case LuaTokenKind.TkString or LuaTokenKind.TkLongString:
                {
                    var m1 = p.Marker();
                    p.Bump();
                    m1.Complete(p, LuaSyntaxKind.LiteralExpr);
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

            return m.Complete(p, LuaSyntaxKind.CallArgList);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.CallArgList, e.Message);
        }
    }
}
