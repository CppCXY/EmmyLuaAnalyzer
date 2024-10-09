using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaStatSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree), ICommentOwner
{
    public static bool CanCast(LuaSyntaxKind kind) =>
        kind is >= LuaSyntaxKind.EmptyStat and <= LuaSyntaxKind.UnknownStat;

    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(this) ?? [];
}

public class LuaLocalStatSyntax : LuaStatSyntax
{
    public LuaLocalStatSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkLocal)
            {
                Local = it.ToToken<LuaSyntaxToken>();
            }
            else if (it.TokenKind == LuaTokenKind.TkAssign)
            {
                Assign = it.ToToken<LuaSyntaxToken>();
            }
            else if (it.Kind == LuaSyntaxKind.LocalName)
            {
                if (it.ToNode<LuaLocalNameSyntax>() is { } localName)
                {
                    NameList.Add(localName);
                }
            }
            else if (LuaExprSyntax.CanCast(it.Kind))
            {
                if (it.ToNode<LuaExprSyntax>() is { } expr)
                {
                    ExprList.Add(expr);
                }
            }
        }
    }

    public LuaSyntaxToken? Local { get; }

    public List<LuaLocalNameSyntax> NameList { get; } = [];

    public LuaSyntaxToken? Assign { get; }

    public List<LuaExprSyntax> ExprList { get; } = [];

    public IEnumerable<(LuaLocalNameSyntax, (LuaExprSyntax?, int))> NameExprPairs
    {
        get
        {
            LuaExprSyntax? lastValidExpr = null;
            var count = NameList.Count;
            var retId = 0;
            for (var i = 0; i < count; i++)
            {
                var localName = NameList[i];
                var expr = ExprList.ElementAtOrDefault(i);
                if (expr is not null)
                {
                    lastValidExpr = expr;
                    retId = 0;
                }
                else
                {
                    retId++;
                }

                yield return (localName, (lastValidExpr, retId));
            }
        }
    }
}

public class LuaAssignStatSyntax : LuaStatSyntax
{
    public LuaAssignStatSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        var foundAssign = false;
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkAssign)
            {
                Assign = it.ToToken<LuaSyntaxToken>();
                foundAssign = true;
            }
            else if (LuaExprSyntax.CanCast(it.Kind))
            {
                if (it.ToNode<LuaExprSyntax>() is { } expr)
                {
                    if (foundAssign)
                    {
                        ExprList.Add(expr);
                    }
                    else
                    {
                        VarList.Add(expr);
                    }
                }
            }
        }
    }

    public List<LuaExprSyntax> VarList { get; } = [];

    public List<LuaExprSyntax> ExprList { get; } = [];

    public LuaSyntaxToken? Assign { get; }

    public IEnumerable<(LuaExprSyntax, (LuaExprSyntax?, int))> VarExprPairs
    {
        get
        {
            LuaExprSyntax? lastValidExpr = null;
            var count = VarList.Count;
            var retId = 0;
            for (var i = 0; i < count; i++)
            {
                var varExpr = VarList[i];
                var expr = ExprList.ElementAtOrDefault(i);
                if (expr is not null)
                {
                    lastValidExpr = expr;
                    retId = 0;
                }
                else
                {
                    retId++;
                }

                yield return (varExpr, (lastValidExpr, retId));
            }
        }
    }
}

public class LuaFuncStatSyntax : LuaStatSyntax
{
    public LuaFuncStatSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkLocal)
            {
                IsLocal = true;
                IsMethod = false;
            }
            else if (it.Kind == LuaSyntaxKind.NameExpr)
            {
                if (it.ToNode<LuaNameExprSyntax>() is { } nameExpr)
                {
                    NameExpr = nameExpr;
                }
            }
            else if (it.Kind == LuaSyntaxKind.IndexExpr)
            {
                if (it.ToNode<LuaIndexExprSyntax>() is { IsColonIndex: { } colonIndex } indexExpr)
                {
                    IndexExpr = indexExpr;
                    IsColonFunc = colonIndex;
                }
            }
            else if (it.Kind == LuaSyntaxKind.ClosureExpr)
            {
                if (it.ToNode<LuaClosureExprSyntax>() is { } closureExpr)
                {
                    ClosureExpr = closureExpr;
                }
            }
            else if (it.Kind == LuaSyntaxKind.LocalName)
            {
                if (it.ToNode<LuaLocalNameSyntax>() is { } localName)
                {
                    LocalName = localName;
                }
            }
        }
    }


    public bool IsLocal { get; }

    public bool IsMethod { get; } = true;

    public bool IsColonFunc { get; }

    public LuaLocalNameSyntax? LocalName { get; }

    public LuaNameExprSyntax? NameExpr { get; }

    public LuaIndexExprSyntax? IndexExpr { get; }

    public LuaClosureExprSyntax? ClosureExpr { get; }

    public LuaSyntaxElement? NameElement
    {
        get
        {
            if (LocalName is  { Name: { } name1 })
            {
                return name1;
            }
            else if (NameExpr is { Name: { } name2 })
            {
                return name2;
            }
            else if (IndexExpr is { KeyElement: { } keyElement })
            {
                return keyElement;
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
