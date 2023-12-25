using System.Diagnostics;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxNode(greenNode, tree, parent)
{
    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(this) ?? Enumerable.Empty<LuaCommentSyntax>();
}

public class LuaLocalStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent)
{
    public LuaSyntaxToken? Local => FirstChildToken(LuaTokenKind.TkLocal);

    public bool IsLocalDeclare => Assign != null;

    public IEnumerable<LuaLocalNameSyntax> NameList => ChildNodes<LuaLocalNameSyntax>();

    public LuaSyntaxToken? Assign => FirstChildToken(LuaTokenKind.TkAssign);

    public IEnumerable<LuaExprSyntax> ExprList => ChildNodes<LuaExprSyntax>();
}

public class LuaAssignStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent)
{
    public IEnumerable<LuaExprSyntax> VarList => ChildNodesBeforeToken<LuaExprSyntax>(LuaTokenKind.TkAssign);

    public IEnumerable<LuaExprSyntax> ExprList => ChildNodesAfterToken<LuaExprSyntax>(LuaTokenKind.TkAssign);

    public LuaSyntaxToken? Assign => FirstChildToken(LuaTokenKind.TkAssign);
}

public class LuaFuncStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent), IFuncBodyOwner
{
    public bool IsLocal => FirstChildToken(LuaTokenKind.TkLocal) != null;

    public bool IsMethod => FirstChild<LuaIndexExprSyntax>() != null;

    public bool IsColonFunc => IndexExpr?.IsColonIndex == true;

    public LuaLocalNameSyntax? LocalName => FirstChild<LuaLocalNameSyntax>();

    public LuaNameExprSyntax? NameExpr => FirstChild<LuaNameExprSyntax>();

    public LuaIndexExprSyntax? IndexExpr => FirstChild<LuaIndexExprSyntax>();

    public LuaFuncBodySyntax? FuncBody => FirstChild<LuaFuncBodySyntax>();
}

public class LuaLabelStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaGotoStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent)
{
    public LuaSyntaxToken? Goto => FirstChildToken(LuaTokenKind.TkGoto);

    public LuaNameToken? LabelName => FirstChild<LuaNameToken>();
}

public class LuaBreakStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent)
{
    public LuaSyntaxToken Break => FirstChildToken(LuaTokenKind.TkBreak)!;
}

public class LuaReturnStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent)
{
    public LuaSyntaxToken Return => FirstChildToken(LuaTokenKind.TkReturn)!;

    public IEnumerable<LuaExprSyntax>? ExpressionSyntaxList => ChildNodes<LuaExprSyntax>();
}

public class LuaIfStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent)
{
    public LuaSyntaxToken If => FirstChildToken(LuaTokenKind.TkIf)!;

    public LuaExprSyntax? Condition => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken? Then => FirstChildToken(LuaTokenKind.TkThen);

    public LuaBlockSyntax? ThenBlock => FirstChild<LuaBlockSyntax>();

    public IEnumerable<LuaIfClauseStatSyntax> IfClauseStatementList => ChildNodes<LuaIfClauseStatSyntax>();

    public LuaSyntaxToken End => FirstChildToken(LuaTokenKind.TkEnd)!;
}

public class LuaIfClauseStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent)
{
    public LuaSyntaxToken? ElseIf => FirstChildToken(LuaTokenKind.TkElseIf);

    public LuaSyntaxToken? Else => FirstChildToken(LuaTokenKind.TkElse);

    public bool IsElseIf => ElseIf != null;

    public bool IsElse => Else != null;

    public LuaExprSyntax? Condition => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken? Then => FirstChildToken(LuaTokenKind.TkThen);

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();
}

public class LuaWhileStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent)
{
    public LuaSyntaxToken While => FirstChildToken(LuaTokenKind.TkWhile)!;

    public LuaExprSyntax? Condition => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken? Do => FirstChildToken(LuaTokenKind.TkDo);

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

    public LuaSyntaxToken? End => FirstChildToken(LuaTokenKind.TkEnd);
}

public class LuaDoStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent)
{
    public LuaSyntaxToken Do => FirstChildToken(LuaTokenKind.TkDo)!;

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

    public LuaSyntaxToken? End => FirstChildToken(LuaTokenKind.TkEnd);
}

public class LuaForStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent)
{
    public LuaParamDefSyntax? IteratorName => FirstChild<LuaParamDefSyntax>();

    public LuaExprSyntax? InitExpr => FirstChild<LuaExprSyntax>();

    public LuaExprSyntax? LimitExpr => ChildNodes<LuaExprSyntax>().Skip(1).FirstOrDefault();

    public LuaExprSyntax? Step => ChildNodes<LuaExprSyntax>().Skip(2).FirstOrDefault();
}

public class LuaForRangeStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent)
{
    public IEnumerable<LuaParamDefSyntax> IteratorNames => ChildNodes<LuaParamDefSyntax>();

    public IEnumerable<LuaExprSyntax> ExprList => ChildNodes<LuaExprSyntax>();

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();
}

public class LuaRepeatStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent)
{
    public LuaSyntaxToken Repeat => FirstChildToken(LuaTokenKind.TkRepeat)!;

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

    public LuaSyntaxToken? Until => FirstChildToken(LuaTokenKind.TkUntil);

    public LuaExprSyntax? Condition => FirstChild<LuaExprSyntax>();
}

public class LuaCallStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent)
{
    public LuaExprSyntax? Expr => FirstChild<LuaExprSyntax>();
}

public class LuaEmptyStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent);

public class LuaUnknownStatSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaStatSyntax(greenNode, tree, parent);
