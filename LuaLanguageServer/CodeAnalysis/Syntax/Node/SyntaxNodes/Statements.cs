using System.Diagnostics;
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
    public IEnumerable<LuaExprSyntax> VarList => ChildNodesBeforeToken<LuaExprSyntax>(LuaTokenKind.TkAssign);

    public IEnumerable<LuaExprSyntax> ExprList => ChildNodesAfterToken<LuaExprSyntax>(LuaTokenKind.TkAssign);

    public LuaSyntaxToken? Assign => FirstChildToken(LuaTokenKind.TkAssign);

    public LuaAssignStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaMethodNameSyntax : LuaSyntaxNode
{
    public bool IsMethod => FirstChild<LuaIndexExprSyntax>() != null;

    public bool IsColonDefine => FirstChild<LuaIndexExprSyntax>()?.IsColonIndex == true;

    public bool IsDotDefine => FirstChild<LuaIndexExprSyntax>()?.IsDotIndex == true;

    public LuaSyntaxToken? Name => FirstChild<LuaExprSyntax>() switch
    {
        LuaNameExprSyntax nameExprSyntax => nameExprSyntax.Name,
        LuaIndexExprSyntax indexExprSyntax => indexExprSyntax.Name,
        _ => throw new UnreachableException()
    };

    public LuaExprSyntax? PrefixExpr => FirstChild<LuaExprSyntax>() switch
    {
        LuaNameExprSyntax => null,
        LuaIndexExprSyntax indexExprSyntax => indexExprSyntax.PrefixExpr,
        _ => throw new UnreachableException()
    };

    public LuaMethodNameSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaFuncStatSyntax : LuaStatSyntax
{
    public bool IsLocal => FirstChildToken(LuaTokenKind.TkLocal) != null;

    public bool IsMethod => FirstChild<LuaMethodNameSyntax>() != null;

    public LuaSyntaxToken? Name =>
        IsLocal ? (LocalName?.Name) : (MethodName?.Name);

    public LuaExprSyntax? PrefixExpr => MethodName?.PrefixExpr;

    public LuaMethodNameSyntax? MethodName => FirstChild<LuaMethodNameSyntax>();

    public LuaLocalNameSyntax? LocalName => FirstChild<LuaLocalNameSyntax>();

    public LuaParamListSyntax? ParamNameList => FirstChild<LuaParamListSyntax>();

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

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
    public LuaParamDef? IteratorName => FirstChild<LuaParamDef>();

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
    public IEnumerable<LuaParamDef> IteratorNames =>ChildNodes<LuaParamDef>();

    public IEnumerable<LuaExprSyntax> ExprList => ChildNodes<LuaExprSyntax>();

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

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
