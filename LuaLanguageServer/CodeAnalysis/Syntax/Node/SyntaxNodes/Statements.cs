using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaStatSyntax : LuaSyntaxNode
{
    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(this) ?? Enumerable.Empty<LuaCommentSyntax>();

    public LuaStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaLocalStatSyntax : LuaStatSyntax
{
    public LuaSyntaxToken? Local => FirstChildToken(LuaTokenKind.TkLocal);

    public bool IsLocalDeclare => Assign != null;

    public IEnumerable<LuaLocalNameSyntax> NameList => ChildNodes<LuaLocalNameSyntax>();

    public LuaSyntaxToken? Assign => FirstChildToken(LuaTokenKind.TkAssign);

    public IEnumerable<LuaExprSyntax> ExpressionList => ChildNodes<LuaExprSyntax>();

    public LuaLocalStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaAssignStatSyntax : LuaStatSyntax
{
    public IEnumerable<LuaVarDefSyntax> VarList => ChildNodes<LuaVarDefSyntax>();

    public IEnumerable<LuaExprSyntax> ExpressionList => ChildNodes<LuaExprSyntax>();

    public LuaSyntaxToken? Assign => FirstChildToken(LuaTokenKind.TkAssign);

    public LuaAssignStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaMethodNameSyntax : LuaSyntaxElement
{
    public bool IsMethod => FirstChild<LuaIndexExprSyntax>() != null;

    public bool IsColonDefine => ChildNodes<LuaIndexExprSyntax>().LastOrDefault()?.IsColonIndex == true;

    public LuaSyntaxToken? Name =>
        IsMethod
            ? ChildNodes<LuaIndexExprSyntax>().LastOrDefault()?.DotOrColonIndexName
            : FirstChild<LuaNameSyntax>()?.Name;

    public LuaExprSyntax? ParentExpr =>
        IsMethod
            ? ChildNodes<LuaIndexExprSyntax>().LastOrDefault()?.ParentExpr
            : null;

    public LuaMethodNameSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaFuncStatSyntax : LuaStatSyntax
{
    public LuaSyntaxToken? Local => FirstChildToken(LuaTokenKind.TkLocal);

    public bool IsLocal => Local != null;

    public bool IsMethod => FirstChild<LuaMethodNameSyntax>() != null;

    public LuaSyntaxToken Function => FirstChildToken(LuaTokenKind.TkFunction)!;

    public LuaSyntaxToken? Name => MethodName?.Name;

    public LuaExprSyntax? ParentExpr => MethodName?.ParentExpr;

    public LuaMethodNameSyntax? MethodName => FirstChild<LuaMethodNameSyntax>();

    public LuaParamListSyntax? ParamNameList => FirstChild<LuaParamListSyntax>();

    public LuaBlockSyntax? BlockSyntax => FirstChild<LuaBlockSyntax>();

    public LuaSyntaxToken? End => FirstChildToken(LuaTokenKind.TkEnd);

    public LuaFuncStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaLabelStatSyntax : LuaStatSyntax
{
    public LuaSyntaxToken? Name => FirstChildToken(LuaTokenKind.TkName);

    public LuaLabelStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaGotoStatSyntax : LuaStatSyntax
{
    public LuaSyntaxToken Goto => FirstChildToken(LuaTokenKind.TkGoto)!;

    public LuaSyntaxToken? LabelName => FirstChildToken(LuaTokenKind.TkName);

    public LuaGotoStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaBreakStatSyntax : LuaStatSyntax
{
    public LuaSyntaxToken Break => FirstChildToken(LuaTokenKind.TkBreak)!;

    public LuaBreakStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaReturnStatSyntax : LuaStatSyntax
{
    public LuaSyntaxToken Return => FirstChildToken(LuaTokenKind.TkReturn)!;

    public IEnumerable<LuaExprSyntax>? ExpressionSyntaxList => ChildNodes<LuaExprSyntax>();

    public LuaReturnStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaIfStatSyntax : LuaStatSyntax
{
    public LuaSyntaxToken If => FirstChildToken(LuaTokenKind.TkIf)!;

    public LuaExprSyntax? Condition => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken? Then => FirstChildToken(LuaTokenKind.TkThen);

    public LuaBlockSyntax? ThenBlock => FirstChild<LuaBlockSyntax>();

    public IEnumerable<LuaIfClauseStatSyntax> IfClauseStatementList => ChildNodes<LuaIfClauseStatSyntax>();

    public LuaSyntaxToken End => FirstChildToken(LuaTokenKind.TkEnd)!;

    public LuaIfStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaIfClauseStatSyntax : LuaStatSyntax
{
    public LuaSyntaxToken? ElseIf => FirstChildToken(LuaTokenKind.TkElseIf);

    public LuaSyntaxToken? Else => FirstChildToken(LuaTokenKind.TkElse);

    public bool IsElseIf => ElseIf != null;

    public bool IsElse => Else != null;

    public LuaExprSyntax? Condition => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken? Then => FirstChildToken(LuaTokenKind.TkThen);

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

    public LuaIfClauseStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaWhileStatSyntax : LuaStatSyntax
{
    public LuaSyntaxToken While => FirstChildToken(LuaTokenKind.TkWhile)!;

    public LuaExprSyntax? Condition => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken? Do => FirstChildToken(LuaTokenKind.TkDo);

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

    public LuaSyntaxToken? End => FirstChildToken(LuaTokenKind.TkEnd);

    public LuaWhileStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDoStatSyntax : LuaStatSyntax
{
    public LuaSyntaxToken Do => FirstChildToken(LuaTokenKind.TkDo)!;

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

    public LuaSyntaxToken? End => FirstChildToken(LuaTokenKind.TkEnd);

    public LuaDoStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaForStatSyntax : LuaStatSyntax
{
    public LuaSyntaxToken For => FirstChildToken(LuaTokenKind.TkFor)!;

    public LuaSyntaxToken? IteratorName => FirstChildToken(LuaTokenKind.TkName);

    public LuaSyntaxToken? Assign => FirstChildToken(LuaTokenKind.TkAssign);

    public LuaExprSyntax? InitExpr => FirstChild<LuaExprSyntax>();

    public LuaExprSyntax? LimitExpr => ChildNodes<LuaExprSyntax>().Skip(1).FirstOrDefault();

    public LuaExprSyntax? Step => ChildNodes<LuaExprSyntax>().Skip(2).FirstOrDefault();

    public LuaForStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaForRangeStatSyntax : LuaStatSyntax
{
    public LuaSyntaxToken For => FirstChildToken(LuaTokenKind.TkFor)!;

    public IEnumerable<LuaSyntaxToken> IteratorNames => ChildTokens(LuaTokenKind.TkName);

    public LuaSyntaxToken? In => FirstChildToken(LuaTokenKind.TkIn);

    public IEnumerable<LuaExprSyntax> ExpressionSyntaxList => ChildNodes<LuaExprSyntax>();

    public LuaSyntaxToken? Do => FirstChildToken(LuaTokenKind.TkDo);

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

    public LuaSyntaxToken? End => FirstChildToken(LuaTokenKind.TkEnd);

    public LuaForRangeStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaRepeatStatSyntax : LuaStatSyntax
{
    public LuaSyntaxToken Repeat => FirstChildToken(LuaTokenKind.TkRepeat)!;

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

    public LuaSyntaxToken? Until => FirstChildToken(LuaTokenKind.TkUntil);

    public LuaExprSyntax? Condition => FirstChild<LuaExprSyntax>();

    public LuaRepeatStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaCallStatSyntax : LuaStatSyntax
{
    public LuaExprSyntax? Expression => FirstChild<LuaExprSyntax>();

    public LuaCallStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaEmptyStatSyntax : LuaStatSyntax
{
    public LuaEmptyStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaUnknownStatSyntax : LuaStatSyntax
{
    public LuaUnknownStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}
