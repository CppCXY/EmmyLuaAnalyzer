using LuaLanguageServer.CodeAnalysis.Compile.Parser;
using LuaLanguageServer.CodeAnalysis.Kind;

namespace LuaLanguageServer.CodeAnalysis.Compile.Grammar.Lua;

public static class StatementParser
{
    public static void Statements(LuaParser p)
    {
        var consumeCount = p.CurrentIndex;
        while (!BlockFollow(p))
        {
            Statement(p);
        }

        if (p.Current is LuaTokenKind.TkEof)
        {
            return;
        }

        if (p.CurrentIndex != consumeCount) return;
        var m = p.Marker();
        var token = p.Current;
        p.Bump();
        m.Fail(p, LuaSyntaxKind.UnknownStat, $"unexpected symbol {token}");
    }

    private static bool BlockFollow(LuaParser p)
    {
        return p.Current is LuaTokenKind.TkElse or LuaTokenKind.TkElseIf or LuaTokenKind.TkEnd
            or LuaTokenKind.TkEof or LuaTokenKind.TkUntil or LuaTokenKind.TkEof;
    }

    private static CompleteMarker Statement(LuaParser p)
    {
        switch (p.Current)
        {
            case LuaTokenKind.TkIf:
                return IfStatement(p);
            case LuaTokenKind.TkWhile:
                return WhileStatement(p);
            case LuaTokenKind.TkDo:
                return DoStatement(p);
            case LuaTokenKind.TkFor:
                return ForStatement(p);
            case LuaTokenKind.TkRepeat:
                return RepeatStatement(p);
            case LuaTokenKind.TkFunction:
                return FunctionStatement(p);
            case LuaTokenKind.TkLocal:
                return LocalStatement(p);
            case LuaTokenKind.TkSemicolon:
                return EmptyStatement(p);
            case LuaTokenKind.TkBreak:
                return BreakStatement(p);
            case LuaTokenKind.TkGoto:
                return GotoStatement(p);
            case LuaTokenKind.TkReturn:
                return ReturnStatement(p);
            case LuaTokenKind.TkDbColon:
                return LabelStatement(p);
            default:
            {
                var consumeCount = p.CurrentIndex;
                var cm = OtherStatement(p);
                // unknown token, skip it
                // ReSharper disable once InvertIf
                if (p.CurrentIndex == consumeCount)
                {
                    var m = p.Marker();
                    var token = p.Current;
                    p.Bump();
                    cm = m.Fail(p, LuaSyntaxKind.UnknownStat, $"unexpected symbol {token}");
                }

                return cm;
            }
        }
    }

    private static CompleteMarker IfStatement(LuaParser p)
    {
        var m = p.Marker();
        p.Bump();
        try
        {
            var condition = ExpressionParser.Expression(p);
            p.Expect(LuaTokenKind.TkThen);

            BlockParser.Block(p);

            while (p.Current is LuaTokenKind.TkElseIf or LuaTokenKind.TkElse)
            {
                IfClause(p);
            }

            p.Expect(LuaTokenKind.TkEnd);
            p.Accept(LuaTokenKind.TkSemicolon);

            return m.Complete(p, LuaSyntaxKind.IfStat);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.IfStat, e.Message);
        }
    }

    private static CompleteMarker IfClause(LuaParser p)
    {
        var m = p.Marker();
        try
        {
            if (p.Current is LuaTokenKind.TkElseIf)
            {
                p.Bump();
                ExpressionParser.Expression(p);
                p.Expect(LuaTokenKind.TkThen);
            }
            else
            {
                p.Expect(LuaTokenKind.TkElse);
            }

            BlockParser.Block(p);

            return m.Complete(p, LuaSyntaxKind.IfClauseStat);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.IfClauseStat, e.Message);
        }
    }

    private static CompleteMarker WhileStatement(LuaParser p)
    {
        var m = p.Marker();
        p.Bump();
        try
        {
            ExpressionParser.Expression(p);
            p.Expect(LuaTokenKind.TkDo);
            BlockParser.Block(p);
            p.Expect(LuaTokenKind.TkEnd);
            p.Accept(LuaTokenKind.TkSemicolon);

            return m.Complete(p, LuaSyntaxKind.WhileStat);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.WhileStat, e.Message);
        }
    }

    private static CompleteMarker DoStatement(LuaParser p)
    {
        var m = p.Marker();
        p.Bump();
        try
        {
            BlockParser.Block(p);
            p.Expect(LuaTokenKind.TkEnd);
            p.Accept(LuaTokenKind.TkSemicolon);

            return m.Complete(p, LuaSyntaxKind.DoStat);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DoStat, e.Message);
        }
    }

    private static CompleteMarker ForStatement(LuaParser p)
    {
        var m = p.Marker();
        p.Bump();
        var kind = LuaSyntaxKind.ForStat;
        try
        {
            p.Expect(LuaTokenKind.TkName);
            if (p.Current == LuaTokenKind.TkAssign)
            {
                p.Bump();
                ExpressionParser.Expression(p);
                p.Expect(LuaTokenKind.TkComma);
                ExpressionParser.Expression(p);
                if (p.Current == LuaTokenKind.TkComma)
                {
                    p.Bump();
                    ExpressionParser.Expression(p);
                }
            }
            else
            {
                kind = LuaSyntaxKind.ForRangeStat;
                while (p.Current is LuaTokenKind.TkComma)
                {
                    p.Bump();
                    p.Expect(LuaTokenKind.TkName);
                }

                p.Expect(LuaTokenKind.TkIn);

                ExpressionParser.Expression(p);
                while (p.Current is LuaTokenKind.TkComma)
                {
                    p.Bump();
                    ExpressionParser.Expression(p);
                }
            }

            p.Expect(LuaTokenKind.TkDo);
            BlockParser.Block(p);
            p.Expect(LuaTokenKind.TkEnd);
            p.Accept(LuaTokenKind.TkSemicolon);

            return m.Complete(p, kind);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, kind, e.Message);
        }
    }

    private static CompleteMarker RepeatStatement(LuaParser p)
    {
        var m = p.Marker();
        p.Bump();
        try
        {
            BlockParser.Block(p);
            p.Expect(LuaTokenKind.TkUntil);
            ExpressionParser.Expression(p);
            p.Accept(LuaTokenKind.TkSemicolon);

            return m.Complete(p, LuaSyntaxKind.RepeatStat);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.RepeatStat, e.Message);
        }
    }

    private static CompleteMarker MethodName(LuaParser p, bool suffix = true)
    {
        var m = p.Marker();
        try
        {
            p.Expect(LuaTokenKind.TkName);

            if (suffix)
            {
                while (p.Current is LuaTokenKind.TkDot)
                {
                    p.Bump();
                    p.Expect(LuaTokenKind.TkName);
                }

                if (p.Current is LuaTokenKind.TkColon)
                {
                    p.Bump();
                    p.Expect(LuaTokenKind.TkName);
                }
            }

            return m.Complete(p, LuaSyntaxKind.MethodName);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.MethodName, e.Message);
        }
    }

    private static CompleteMarker FunctionStatement(LuaParser p)
    {
        var m = p.Marker();
        p.Bump();
        try
        {
            MethodName(p);
            FunctionBody(p);
            p.Accept(LuaTokenKind.TkSemicolon);

            return m.Complete(p, LuaSyntaxKind.FuncStat);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.FuncStat, e.Message);
        }
    }

    private static CompleteMarker ParamList(LuaParser p)
    {
        var m = p.Marker();
        try
        {
            p.Expect(LuaTokenKind.TkLeftParen);
            if (p.Current is LuaTokenKind.TkName)
            {
                p.Bump();
                while (p.Current is LuaTokenKind.TkComma)
                {
                    p.Bump();
                    if (p.Current == LuaTokenKind.TkName)
                    {
                        p.Bump();
                    }
                    else
                    {
                        p.Expect(LuaTokenKind.TkDots);
                        break;
                    }
                }
            }
            else if (p.Current is LuaTokenKind.TkDots)
            {
                p.Bump();
            }
            else if (p.Current is LuaTokenKind.TkRightParen)
            {
                // ignore
            }
            else
            {
                throw new UnexpectedTokenException("expected name or '...' in paramList");
            }

            p.Expect(LuaTokenKind.TkRightParen);
            return m.Complete(p, LuaSyntaxKind.ParamList);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.ParamList, e.Message);
        }
    }

    public static void FunctionBody(LuaParser p)
    {
        if (ParamList(p).IsComplete)
        {
            BlockParser.Block(p);
            p.Expect(LuaTokenKind.TkEnd);
        }
    }

    private static CompleteMarker LocalStatement(LuaParser p)
    {
        var m = p.Marker();
        p.Bump();
        var kind = LuaSyntaxKind.LocalStat;
        try
        {
            if (p.Current == LuaTokenKind.TkFunction)
            {
                kind = LuaSyntaxKind.LocalFuncStat;
                p.Bump();
                MethodName(p, false);
                FunctionBody(p);
            }
            else
            {
                p.Expect(LuaTokenKind.TkName);
                if (p.Current is LuaTokenKind.TkLt)
                {
                    LocalAttribute(p);
                }

                while (p.Current is LuaTokenKind.TkComma)
                {
                    p.Bump();
                    p.Expect(LuaTokenKind.TkName);
                    if (p.Current is LuaTokenKind.TkLt)
                    {
                        LocalAttribute(p);
                    }
                }

                if (p.Current == LuaTokenKind.TkAssign)
                {
                    p.Bump();
                    ExpressionParser.Expression(p);
                    while (p.Current is LuaTokenKind.TkComma)
                    {
                        p.Bump();
                        ExpressionParser.Expression(p);
                    }
                }
            }

            p.Accept(LuaTokenKind.TkSemicolon);
            return m.Complete(p, kind);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, kind, e.Message);
        }
    }

    private static CompleteMarker LocalAttribute(LuaParser p)
    {
        var m = p.Marker();
        p.Bump();
        try
        {
            if (p.CurrentName is not ("const" or "close"))
            {
                return m.Fail(p, LuaSyntaxKind.Attribute, "expected const or close");
            }

            p.Bump();
            p.Expect(LuaTokenKind.TkGt);
            return m.Complete(p, LuaSyntaxKind.Attribute);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.Attribute, e.Message);
        }
    }

    private static CompleteMarker EmptyStatement(LuaParser p)
    {
        var m = p.Marker();
        p.Bump();
        return m.Complete(p, LuaSyntaxKind.EmptyStat);
    }

    private static CompleteMarker BreakStatement(LuaParser p)
    {
        var m = p.Marker();
        p.Bump();
        p.Accept(LuaTokenKind.TkSemicolon);

        return m.Complete(p, LuaSyntaxKind.BreakStat);
    }

    private static CompleteMarker GotoStatement(LuaParser p)
    {
        var m = p.Marker();
        p.Bump();
        try
        {
            p.Expect(LuaTokenKind.TkName);
            p.Accept(LuaTokenKind.TkSemicolon);

            return m.Complete(p, LuaSyntaxKind.GotoStat);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.GotoStat, e.Message);
        }
    }

    private static CompleteMarker ReturnStatement(LuaParser p)
    {
        var m = p.Marker();
        p.Bump();
        try
        {
            if (p.Current is not LuaTokenKind.TkSemicolon && !BlockFollow(p))
            {
                ExpressionParser.Expression(p);
                while (p.Current is LuaTokenKind.TkComma)
                {
                    p.Bump();
                    ExpressionParser.Expression(p);
                }
            }

            p.Accept(LuaTokenKind.TkSemicolon);

            return m.Complete(p, LuaSyntaxKind.ReturnStat);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.ReturnStat, e.Message);
        }
    }

    private static CompleteMarker LabelStatement(LuaParser p)
    {
        var m = p.Marker();
        p.Bump();
        try
        {
            p.Expect(LuaTokenKind.TkName);
            p.Expect(LuaTokenKind.TkDbColon);

            return m.Complete(p, LuaSyntaxKind.LabelStat);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.LabelStat, e.Message);
        }
    }

    private static CompleteMarker OtherStatement(LuaParser p)
    {
        var m = p.Marker();
        try
        {
            var cm = ExpressionParser.SuffixExpression(p);
            if (!cm.IsComplete)
            {
                return m.Fail(p, LuaSyntaxKind.ExprStat, "expected expression");
            }

            if (p.Current is not (LuaTokenKind.TkAssign or LuaTokenKind.TkComma))
            {
                return m.Complete(p, LuaSyntaxKind.ExprStat);
            }

            var m1 = cm.Precede(p);
            m1.Complete(p, LuaSyntaxKind.VarDef);
            while (p.Current is LuaTokenKind.TkComma)
            {
                p.Bump();
                ExpressionParser.VarDefinition(p);
            }

            p.Expect(LuaTokenKind.TkAssign);

            ExpressionParser.Expression(p);
            while (p.Current is LuaTokenKind.TkComma)
            {
                p.Bump();
                ExpressionParser.Expression(p);
            }

            p.Accept(LuaTokenKind.TkSemicolon);

            return m.Complete(p, LuaSyntaxKind.AssignStat);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.AssignStat, e.Message);
        }
    }
}
