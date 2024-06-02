using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaStatSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(this) ?? [];
}

public class LuaLocalStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public LuaSyntaxToken? Local => FirstChildToken(LuaTokenKind.TkLocal);

    public bool IsLocalDeclare => Assign != null;

    public IEnumerable<LuaLocalNameSyntax> NameList => ChildrenElement<LuaLocalNameSyntax>();

    public LuaSyntaxToken? Assign => FirstChildToken(LuaTokenKind.TkAssign);

    public IEnumerable<LuaExprSyntax> ExprList => ChildrenElement<LuaExprSyntax>();
}

public class LuaAssignStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public IEnumerable<LuaExprSyntax> VarList => ChildNodesBeforeToken<LuaExprSyntax>(LuaTokenKind.TkAssign);

    public IEnumerable<LuaExprSyntax> ExprList => ChildNodesAfterToken<LuaExprSyntax>(LuaTokenKind.TkAssign);

    public LuaSyntaxToken? Assign => FirstChildToken(LuaTokenKind.TkAssign);
}

public class LuaFuncStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public bool IsLocal => FirstChildToken(LuaTokenKind.TkLocal) != null;

    public bool IsMethod => FirstChild<LuaIndexExprSyntax>() != null;

    public bool IsColonFunc => IndexExpr?.IsColonIndex == true;

    public LuaLocalNameSyntax? LocalName => FirstChild<LuaLocalNameSyntax>();

    public LuaNameExprSyntax? NameExpr => FirstChild<LuaNameExprSyntax>();

    public LuaIndexExprSyntax? IndexExpr => FirstChild<LuaIndexExprSyntax>();

    public LuaClosureExprSyntax? ClosureExpr => FirstChild<LuaClosureExprSyntax>();

    public LuaSyntaxElement? NameElement
    {
        get
        {
            foreach (var element in ChildrenElements)
            {
                if (element is LuaLocalNameSyntax { Name: { } name1 })
                {
                    return name1;
                }
                else if (element is LuaNameExprSyntax { Name: { } name2 })
                {
                    return name2;
                }
                else if (element is LuaIndexExprSyntax { KeyElement: { } keyElement })
                {
                    return keyElement;
                }
            }

            return null;
        }
    }
}

public class LuaLabelStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaGotoStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public LuaSyntaxToken Goto => FirstChildToken(LuaTokenKind.TkGoto)!;

    public LuaNameToken? LabelName => FirstChild<LuaNameToken>();
}

public class LuaBreakStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public LuaSyntaxToken Break => FirstChildToken(LuaTokenKind.TkBreak)!;
}

public class LuaReturnStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public LuaSyntaxToken Return => FirstChildToken(LuaTokenKind.TkReturn)!;

    public IEnumerable<LuaExprSyntax> ExprList => ChildrenElement<LuaExprSyntax>();
}

public class LuaIfStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public LuaSyntaxToken If => FirstChildToken(LuaTokenKind.TkIf)!;

    public LuaExprSyntax? Condition => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken? Then => FirstChildToken(LuaTokenKind.TkThen);

    public LuaBlockSyntax? ThenBlock => FirstChild<LuaBlockSyntax>();

    public IEnumerable<LuaIfClauseStatSyntax> IfClauseStatementList => ChildrenElement<LuaIfClauseStatSyntax>();

    public LuaSyntaxToken End => FirstChildToken(LuaTokenKind.TkEnd)!;
}

public class LuaIfClauseStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public LuaSyntaxToken? ElseIf => FirstChildToken(LuaTokenKind.TkElseIf);

    public LuaSyntaxToken? Else => FirstChildToken(LuaTokenKind.TkElse);

    public bool IsElseIf => ElseIf != null;

    public bool IsElse => Else != null;

    public LuaExprSyntax? Condition => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken? Then => FirstChildToken(LuaTokenKind.TkThen);

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();
}

public class LuaWhileStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public LuaSyntaxToken While => FirstChildToken(LuaTokenKind.TkWhile)!;

    public LuaExprSyntax? Condition => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken? Do => FirstChildToken(LuaTokenKind.TkDo);

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

    public LuaSyntaxToken? End => FirstChildToken(LuaTokenKind.TkEnd);
}

public class LuaDoStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public LuaSyntaxToken Do => FirstChildToken(LuaTokenKind.TkDo)!;

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

    public LuaSyntaxToken? End => FirstChildToken(LuaTokenKind.TkEnd);
}

public class LuaForStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public LuaParamDefSyntax? IteratorName => FirstChild<LuaParamDefSyntax>();

    public LuaExprSyntax? InitExpr => FirstChild<LuaExprSyntax>();

    public LuaExprSyntax? LimitExpr => ChildrenElement<LuaExprSyntax>().Skip(1).FirstOrDefault();

    public LuaExprSyntax? Step => ChildrenElement<LuaExprSyntax>().Skip(2).FirstOrDefault();

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();
}

public class LuaForRangeStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public IEnumerable<LuaParamDefSyntax> IteratorNames => ChildrenElement<LuaParamDefSyntax>();

    public IEnumerable<LuaExprSyntax> ExprList => ChildrenElement<LuaExprSyntax>();

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();
}

public class LuaRepeatStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public LuaSyntaxToken Repeat => FirstChildToken(LuaTokenKind.TkRepeat)!;

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

    public LuaSyntaxToken? Until => FirstChildToken(LuaTokenKind.TkUntil);

    public LuaExprSyntax? Condition => FirstChild<LuaExprSyntax>();
}

public class LuaCallStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public LuaExprSyntax? Expr => FirstChild<LuaExprSyntax>();
}

public class LuaEmptyStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree);

public class LuaUnknownStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree);
