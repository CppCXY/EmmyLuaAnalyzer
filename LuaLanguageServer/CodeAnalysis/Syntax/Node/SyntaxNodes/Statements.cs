using LuaLanguageServer.CodeAnalysis.Syntax.Green;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaStatementSyntax : LuaSyntaxNode
{
    public LuaStatementSyntax(GreenNode greenNode)
        : base(greenNode)
    {
    }
}

public class LuaLocalStatementSyntax : LuaStatementSyntax
{
    public LuaSyntaxToken Local { get; }

    public List<NameSyntax> NameSyntaxList { get; }

    public LuaSyntaxToken? Assign { get; }

    public List<ExpressionSyntax>? ExpressionSyntaxList { get; }

    public LuaLocalStatementSyntax(GreenNode greenNode, LuaSyntaxToken local, List<NameSyntax> nameList,
        LuaSyntaxToken? assign, List<ExpressionSyntax>? expressionList)
        : base(greenNode)
    {
        Local = local;
        NameSyntaxList = nameList;
        Assign = assign;
        ExpressionSyntaxList = expressionList;
    }
}

public class LuaAssignmentStatementSyntax : LuaStatementSyntax
{
    public List<VarDefSyntax> VarList { get; }

    public List<ExpressionSyntax> ExpressionSyntaxList { get; }

    public LuaSyntaxToken Assign { get; }

    public LuaAssignmentStatementSyntax(GreenNode greenNode, List<VarDefSyntax> varList, LuaSyntaxToken assign,
        List<ExpressionSyntax> expressionList)
        : base(greenNode)
    {
        VarList = varList;
        Assign = assign;
        ExpressionSyntaxList = expressionList;
    }
}

public class LuaFunctionStatementSyntax : LuaStatementSyntax
{
    public LuaSyntaxToken Local { get; }

    public LuaSyntaxToken Function { get; }

    public LuaSyntaxToken Name { get; }

    public LuaSyntaxToken OpenParen { get; }

    public List<LuaSyntaxToken> ParamNameList { get; }

    public LuaSyntaxToken CloseParen { get; }

    public LuaBlockSyntax BlockSyntax { get; }

    public LuaSyntaxToken End { get; }

    public LuaFunctionStatementSyntax(GreenNode greenNode, LuaSyntaxToken local, LuaSyntaxToken function,
        LuaSyntaxToken name, LuaSyntaxToken openParen, List<LuaSyntaxToken> paramNameList, LuaSyntaxToken closeParen,
        LuaBlockSyntax blockSyntax, LuaSyntaxToken end)
        : base(greenNode)
    {
        Local = local;
        Function = function;
        Name = name;
        OpenParen = openParen;
        ParamNameList = paramNameList;
        CloseParen = closeParen;
        BlockSyntax = blockSyntax;
        End = end;
    }
}

public class LuaLabelStatementSyntax : LuaStatementSyntax
{
    public LuaSyntaxToken LeftDoubleColon { get; }

    public LuaSyntaxToken Name { get; }

    public LuaSyntaxToken RightDoubleColon { get; }

    public LuaLabelStatementSyntax(GreenNode greenNode, LuaSyntaxToken leftDoubleColon, LuaSyntaxToken name,
        LuaSyntaxToken rightDoubleColon)
        : base(greenNode)
    {
        LeftDoubleColon = leftDoubleColon;
        Name = name;
        RightDoubleColon = rightDoubleColon;
    }
}

public class LuaGotoStatementSyntax : LuaStatementSyntax
{
    public LuaSyntaxToken Goto { get; }

    public LuaSyntaxToken LabelName { get; }

    public LuaGotoStatementSyntax(GreenNode greenNode, LuaSyntaxToken @goto, LuaSyntaxToken labelName)
        : base(greenNode)
    {
        Goto = @goto;
        LabelName = labelName;
    }
}

public class LuaBreakStatementSyntax : LuaStatementSyntax
{
    public LuaSyntaxToken Break { get; }

    public LuaBreakStatementSyntax(GreenNode greenNode, LuaSyntaxToken @break)
        : base(greenNode)
    {
        Break = @break;
    }
}

public class LuaReturnStatementSyntax : LuaStatementSyntax
{
    public LuaSyntaxToken Return { get; }

    public List<ExpressionSyntax>? ExpressionSyntaxList { get; }

    public LuaReturnStatementSyntax(GreenNode greenNode, LuaSyntaxToken @return,
        List<ExpressionSyntax>? expressionSyntaxList)
        : base(greenNode)
    {
        Return = @return;
        ExpressionSyntaxList = expressionSyntaxList;
    }
}

public class LuaIfStatementSyntax : LuaStatementSyntax
{
    public LuaSyntaxToken If { get; }

    public ExpressionSyntax Condition { get; }

    public LuaSyntaxToken Then { get; }

    public LuaBlockSyntax ThenBlock { get; }

    public List<LuaElseIfStatementSyntax>? ElseIfStatementSyntaxList { get; }

    public LuaSyntaxToken? Else { get; }

    public LuaBlockSyntax? ElseBlock { get; }

    public LuaSyntaxToken End { get; }

    public LuaIfStatementSyntax(GreenNode greenNode, LuaSyntaxToken @if, ExpressionSyntax condition,
        LuaSyntaxToken then, LuaBlockSyntax thenBlock, List<LuaElseIfStatementSyntax>? elseIfStatementSyntaxList,
        LuaSyntaxToken? @else, LuaBlockSyntax? elseBlock, LuaSyntaxToken end)
        : base(greenNode)
    {
        If = @if;
        Condition = condition;
        Then = then;
        ThenBlock = thenBlock;
        ElseIfStatementSyntaxList = elseIfStatementSyntaxList;
        Else = @else;
        ElseBlock = elseBlock;
        End = end;
    }
}

public class LuaElseIfStatementSyntax : LuaStatementSyntax
{
    public LuaSyntaxToken ElseIf { get; }

    public ExpressionSyntax Condition { get; }

    public LuaSyntaxToken Then { get; }

    public LuaBlockSyntax ThenBlock { get; }

    public LuaElseIfStatementSyntax(GreenNode greenNode, LuaSyntaxToken elseIf, ExpressionSyntax condition,
        LuaSyntaxToken then, LuaBlockSyntax thenBlock)
        : base(greenNode)
    {
        ElseIf = elseIf;
        Condition = condition;
        Then = then;
        ThenBlock = thenBlock;
    }
}

public class LuaWhileStatementSyntax : LuaStatementSyntax
{
    public LuaSyntaxToken While { get; }

    public ExpressionSyntax Condition { get; }

    public LuaSyntaxToken Do { get; }

    public LuaBlockSyntax Block { get; }

    public LuaSyntaxToken End { get; }

    public LuaWhileStatementSyntax(GreenNode greenNode, LuaSyntaxToken @while, ExpressionSyntax condition,
        LuaSyntaxToken @do, LuaBlockSyntax block, LuaSyntaxToken end)
        : base(greenNode)
    {
        While = @while;
        Condition = condition;
        Do = @do;
        Block = block;
        End = end;
    }
}

public class LuaDoStatementSyntax : LuaStatementSyntax
{
    public LuaSyntaxToken Do { get; }

    public LuaBlockSyntax Block { get; }

    public LuaSyntaxToken End { get; }

    public LuaDoStatementSyntax(GreenNode greenNode, LuaSyntaxToken @do, LuaBlockSyntax block, LuaSyntaxToken end)
        : base(greenNode)
    {
        Do = @do;
        Block = block;
        End = end;
    }
}

public class LuaForStatementSyntax : LuaStatementSyntax
{
    public LuaSyntaxToken For { get; }

    public NameSyntax IteratorName { get; }

    public LuaSyntaxToken Assign { get; }

    public ExpressionSyntax InitExpr { get; }

    public LuaSyntaxToken Comma1 { get; }

    public ExpressionSyntax LimitExpr { get; }

    public LuaSyntaxToken? Comma2 { get; }

    public ExpressionSyntax? Step { get; }

    public LuaForStatementSyntax(GreenNode greenNode, LuaSyntaxToken @for, NameSyntax iteratorName,
        LuaSyntaxToken assign, ExpressionSyntax initExpr, LuaSyntaxToken comma1, ExpressionSyntax limitExpr,
        LuaSyntaxToken? comma2, ExpressionSyntax? step)
        : base(greenNode)
    {
        For = @for;
        IteratorName = iteratorName;
        Assign = assign;
        InitExpr = initExpr;
        Comma1 = comma1;
        LimitExpr = limitExpr;
        Comma2 = comma2;
        Step = step;
    }
}

public class LuaForEachStatementSyntax : LuaStatementSyntax
{
    public LuaForEachStatementSyntax(GreenNode greenNode)
        : base(greenNode)
    {
    }
}

public class LuaRepeatStatementSyntax : LuaStatementSyntax
{
    public LuaRepeatStatementSyntax(GreenNode greenNode)
        : base(greenNode)
    {
    }
}

public class LuaFunctionCallStatementSyntax : LuaStatementSyntax
{
    public LuaFunctionCallStatementSyntax(GreenNode greenNode)
        : base(greenNode)
    {
    }
}

