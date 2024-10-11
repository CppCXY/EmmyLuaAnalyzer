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
                _localTokenIndex = it.Index;
            }
            else if (it.TokenKind == LuaTokenKind.TkAssign)
            {
                _assignTokenIndex = it.Index;
            }
            else if (it.Kind == LuaSyntaxKind.LocalName)
            {
                _nameListIndex.Add(it.Index);
            }
            else if (LuaExprSyntax.CanCast(it.Kind))
            {
                _exprListIndex.Add(it.Index);
            }
        }
    }


    private int _localTokenIndex = -1;

    public LuaSyntaxToken? Local => Tree.GetElement<LuaSyntaxToken>(_localTokenIndex);

    private List<int> _nameListIndex = [];

    public IEnumerable<LuaLocalNameSyntax> NameList => Tree.GetElements<LuaLocalNameSyntax>(_nameListIndex);

    private int _assignTokenIndex = -1;

    public LuaSyntaxToken? Assign => Tree.GetElement<LuaSyntaxToken>(_assignTokenIndex);

    private List<int> _exprListIndex = [];

    public IEnumerable<LuaExprSyntax> ExprList => Tree.GetElements<LuaExprSyntax>(_exprListIndex);

    public IEnumerable<(LuaLocalNameSyntax, (LuaExprSyntax?, int))> NameExprPairs
    {
        get
        {
            var nameList = NameList.ToList();
            var exprList = ExprList.ToList();
            LuaExprSyntax? lastValidExpr = null;
            var count = nameList.Count;
            var retId = 0;
            for (var i = 0; i < count; i++)
            {
                var localName = nameList[i];
                var expr = exprList.ElementAtOrDefault(i);
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
                _assignTokenIndex = it.Index;
                foundAssign = true;
            }
            else if (LuaExprSyntax.CanCast(it.Kind))
            {
                if (foundAssign)
                {
                    _exprListIndex.Add(it.Index);
                }
                else
                {
                    _varListIndex.Add(it.Index);
                }
            }
        }
    }

    private List<int> _varListIndex = [];

    public IEnumerable<LuaExprSyntax> VarList => Tree.GetElements<LuaExprSyntax>(_varListIndex);

    private List<int> _exprListIndex = [];

    public IEnumerable<LuaExprSyntax> ExprList => Tree.GetElements<LuaExprSyntax>(_exprListIndex);

    private int _assignTokenIndex = -1;

    public LuaSyntaxToken? Assign => Tree.GetElement<LuaSyntaxToken>(_assignTokenIndex);

    public IEnumerable<(LuaExprSyntax, (LuaExprSyntax?, int))> VarExprPairs
    {
        get
        {
            var varList = VarList.ToList();
            var exprList = ExprList.ToList();

            LuaExprSyntax? lastValidExpr = null;
            var count = varList.Count;
            var retId = 0;
            for (var i = 0; i < count; i++)
            {
                var varExpr = varList[i];
                var expr = exprList.ElementAtOrDefault(i);
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
    enum LuaFuncStatState
    {
        None,
        Local,
        Global,
        DotMethod,
        ColonMethod,
    }

    public LuaFuncStatSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        _state = LuaFuncStatState.None;
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkLocal)
            {
                _state = LuaFuncStatState.Local;
            }
            else if (it.Kind == LuaSyntaxKind.NameExpr)
            {
                if (_state == LuaFuncStatState.None)
                {
                    _state = LuaFuncStatState.Global;
                }

                _funcNameIndex = it.Index;
            }
            else if (it.Kind == LuaSyntaxKind.IndexExpr)
            {
                _state = LuaFuncStatState.DotMethod;
                foreach (var it2 in it.ChildrenTokens)
                {
                    if (it2.TokenKind == LuaTokenKind.TkColon)
                    {
                        _state = LuaFuncStatState.ColonMethod;
                        break;
                    }
                }

                _funcNameIndex = it.Index;
            }
            else if (it.Kind == LuaSyntaxKind.ClosureExpr)
            {
                _closureExprIndex = it.Index;
            }
            else if (it.Kind == LuaSyntaxKind.LocalName)
            {
                _funcNameIndex = it.Index;
            }
        }
    }

    private LuaFuncStatState _state;

    public bool IsLocal => _state == LuaFuncStatState.Local;

    public bool IsGlobal => _state == LuaFuncStatState.Global;

    public bool IsMethod => _state is LuaFuncStatState.DotMethod or LuaFuncStatState.ColonMethod;

    public bool IsColonMethod => _state == LuaFuncStatState.ColonMethod;

    public bool IsDotMethod => _state == LuaFuncStatState.DotMethod;

    private int _funcNameIndex = -1;

    public LuaLocalNameSyntax? LocalName => Tree.GetElement<LuaLocalNameSyntax>(_funcNameIndex);

    public LuaNameExprSyntax? NameExpr => Tree.GetElement<LuaNameExprSyntax>(_funcNameIndex);

    public LuaIndexExprSyntax? IndexExpr => Tree.GetElement<LuaIndexExprSyntax>(_funcNameIndex);

    private int _closureExprIndex = -1;

    public LuaClosureExprSyntax? ClosureExpr => Tree.GetElement<LuaClosureExprSyntax>(_closureExprIndex);

    public LuaSyntaxElement? NameElement
    {
        get
        {
            if (IsLocal && LocalName is { Name: { } name1 })
            {
                return name1;
            }
            else if (IsGlobal && NameExpr is { Name: { } name2 })
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
    public LuaNameToken? Name => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();
}

public class LuaGotoStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public LuaSyntaxToken Goto => Iter.FirstChildToken(LuaTokenKind.TkGoto).ToToken<LuaSyntaxToken>()!;

    public LuaNameToken? LabelName => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();
}

public class LuaBreakStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree)
{
    public LuaSyntaxToken Break => Iter.FirstChildToken(LuaTokenKind.TkBreak).ToToken<LuaSyntaxToken>()!;
}

public class LuaReturnStatSyntax : LuaStatSyntax
{
    public LuaReturnStatSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkReturn)
            {
                _returnTokenIndex = it.Index;
            }
            else if (LuaExprSyntax.CanCast(it.Kind))
            {
                _exprListIndex.Add(it.Index);
            }
        }
    }

    private int _returnTokenIndex = -1;

    public LuaSyntaxToken Return => Tree.GetElement<LuaSyntaxToken>(_returnTokenIndex)!;

    private List<int> _exprListIndex = [];

    public IEnumerable<LuaExprSyntax> ExprList => Tree.GetElements<LuaExprSyntax>(_exprListIndex);
}

public class LuaIfStatSyntax : LuaStatSyntax
{
    [Flags]
    enum LuaIfStatState
    {
        None = 0,
        HasElse = 1,
        HasElseIf = 2,
    }

    public LuaIfStatSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        _state = LuaIfStatState.None;
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkIf)
            {
                _ifTokenIndex = it.Index;
            }
            else if (it.TokenKind == LuaTokenKind.TkThen)
            {
                _thenTokenIndex = it.Index;
            }
            else if (it.TokenKind == LuaTokenKind.TkEnd)
            {
                _endTokenIndex = it.Index;
            }
            else if (it.Kind == LuaSyntaxKind.IfClauseStat)
            {
                _ifClauseStatIndex.Add(it.Index);
                if (it.FirstChildToken(i => i is LuaTokenKind.TkElse or LuaTokenKind.TkElseIf) is { } token)
                {
                    if (token.TokenKind == LuaTokenKind.TkElse)
                    {
                        _state |= LuaIfStatState.HasElse;
                    }
                    else if (token.TokenKind == LuaTokenKind.TkElseIf)
                    {
                        _state |= LuaIfStatState.HasElseIf;
                    }
                }
            }
        }
    }

    private LuaIfStatState _state;

    public bool HasElse => _state.HasFlag(LuaIfStatState.HasElse);

    public bool HasElseIf => _state.HasFlag(LuaIfStatState.HasElseIf);

    private int _ifTokenIndex = -1;

    public LuaSyntaxToken If => Tree.GetElement<LuaSyntaxToken>(_ifTokenIndex)!;

    private int _conditionIndex = -1;

    public LuaExprSyntax? Condition => Tree.GetElement<LuaExprSyntax>(_conditionIndex);

    private int _thenTokenIndex = -1;

    public LuaSyntaxToken? Then => Tree.GetElement<LuaSyntaxToken>(_thenTokenIndex);

    private int _blockIndex = -1;

    public LuaBlockSyntax? ThenBlock => Tree.GetElement<LuaBlockSyntax>(_blockIndex);

    private List<int> _ifClauseStatIndex = [];

    public IEnumerable<LuaIfClauseStatSyntax> IfClauseStatementList =>
        Tree.GetElements<LuaIfClauseStatSyntax>(_ifClauseStatIndex);

    private int _endTokenIndex = -1;

    public LuaSyntaxToken? End => Tree.GetElement<LuaSyntaxToken>(_endTokenIndex);
}

public class LuaIfClauseStatSyntax : LuaStatSyntax
{
    enum LuaIfClauseStatState
    {
        None,
        ElseIf,
        Else,
    }

    public LuaIfClauseStatSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkElseIf)
            {
                _keywordIndex = it.Index;
                _state = LuaIfClauseStatState.ElseIf;
            }
            else if (it.TokenKind == LuaTokenKind.TkElse)
            {
                _keywordIndex = it.Index;
                _state = LuaIfClauseStatState.Else;
            }
            else if (LuaExprSyntax.CanCast(it.Kind))
            {
                _conditionIndex = it.Index;
            }
            else if (it.Kind == LuaSyntaxKind.Block)
            {
                _blockIndex = it.Index;
            }
        }
    }

    private LuaIfClauseStatState _state;

    public bool IsElseIf => _state == LuaIfClauseStatState.ElseIf;

    public bool IsElse => _state == LuaIfClauseStatState.Else;

    private int _keywordIndex = -1;

    public LuaSyntaxToken? ElseIf => Tree.GetElement<LuaSyntaxToken>(_keywordIndex);

    public LuaSyntaxToken? Else => Tree.GetElement<LuaSyntaxToken>(_keywordIndex);

    private int _conditionIndex = -1;

    public LuaExprSyntax? Condition => Tree.GetElement<LuaExprSyntax>(_conditionIndex);

    private int _blockIndex = -1;

    public LuaBlockSyntax? Block => Tree.GetElement<LuaBlockSyntax>(_blockIndex);
}

public class LuaWhileStatSyntax : LuaStatSyntax
{
    public LuaWhileStatSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkWhile)
            {
                _whileTokenIndex = it.Index;
            }
            else if (LuaExprSyntax.CanCast(it.Kind))
            {
                _conditionIndex = it.Index;
            }
            else if (it.Kind == LuaSyntaxKind.Block)
            {
                _blockIndex = it.Index;
            }
        }
    }

    private int _whileTokenIndex = -1;

    public LuaSyntaxToken While => Tree.GetElement<LuaSyntaxToken>(_whileTokenIndex)!;

    private int _conditionIndex = -1;

    public LuaExprSyntax? Condition => Tree.GetElement<LuaExprSyntax>(_conditionIndex);

    private int _blockIndex = -1;

    public LuaBlockSyntax? Block => Tree.GetElement<LuaBlockSyntax>(_blockIndex);
}

public class LuaDoStatSyntax: LuaStatSyntax
{
    public LuaDoStatSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkDo)
            {
                _doTokenIndex = it.Index;
            }
            else if (it.Kind == LuaSyntaxKind.Block)
            {
                _blockIndex = it.Index;
            }
        }
    }

    private int _doTokenIndex = -1;

    public LuaSyntaxToken Do => Tree.GetElement<LuaSyntaxToken>(_doTokenIndex)!;

    private int _blockIndex = -1;

    public LuaBlockSyntax? Block => Tree.GetElement<LuaBlockSyntax>(_blockIndex);
}

public class LuaForStatSyntax : LuaStatSyntax
{
    public LuaForStatSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.Kind == LuaSyntaxKind.ParamName)
            {
                _itIndex = it.Index;
            }
            else if (LuaExprSyntax.CanCast(it.Kind))
            {
                if (_initExprIndex == -1)
                {
                    _initExprIndex = it.Index;
                }
                else if (_limitExpIndex == -1)
                {
                    _limitExpIndex = it.Index;
                }
                else if (_stepExpIndex == -1)
                {
                    _stepExpIndex = it.Index;
                }
            }
            else if (it.Kind == LuaSyntaxKind.Block)
            {
                _blockIndex = it.Index;
            }
        }
    }

    private int _itIndex = -1;

    public LuaParamDefSyntax? IteratorName => Tree.GetElement<LuaParamDefSyntax>(_itIndex);

    private int _initExprIndex = -1;

    public LuaExprSyntax? InitExpr => Tree.GetElement<LuaExprSyntax>(_initExprIndex);

    private int _limitExpIndex = -1;

    public LuaExprSyntax? LimitExpr => Tree.GetElement<LuaExprSyntax>(_limitExpIndex);

    private int _stepExpIndex = -1;

    public LuaExprSyntax? Step => Tree.GetElement<LuaExprSyntax>(_stepExpIndex);

    private int _blockIndex = -1;

    public LuaBlockSyntax? Block => Tree.GetElement<LuaBlockSyntax>(_blockIndex);
}

public class LuaForRangeStatSyntax: LuaStatSyntax
{
    public LuaForRangeStatSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.Kind == LuaSyntaxKind.ParamName)
            {
                _itNamesIndex.Add(it.Index);
            }
            else if (LuaExprSyntax.CanCast(it.Kind))
            {
                _exprListIndex.Add(it.Index);
            }
            else if (it.Kind == LuaSyntaxKind.Block)
            {
                _blockIndex = it.Index;
            }
        }
    }

    private List<int> _itNamesIndex = [];

    public IEnumerable<LuaParamDefSyntax> IteratorNames => Tree.GetElements<LuaParamDefSyntax>(_itNamesIndex);

    private List<int> _exprListIndex = [];

    public IEnumerable<LuaExprSyntax> ExprList => Tree.GetElements<LuaExprSyntax>(_exprListIndex);

    private int _blockIndex = -1;

    public LuaBlockSyntax? Block => Tree.GetElement<LuaBlockSyntax>(_blockIndex);
}

public class LuaRepeatStatSyntax : LuaStatSyntax
{
    public LuaRepeatStatSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkRepeat)
            {
                _repeatTokenIndex = it.Index;
            }
            else if (it.TokenKind == LuaTokenKind.TkUntil)
            {
                _untilTokenIndex = it.Index;
            }
            else if (LuaExprSyntax.CanCast(it.Kind))
            {
                _conditionIndex = it.Index;
            }
            else if (it.Kind == LuaSyntaxKind.Block)
            {
                _blockIndex = it.Index;
            }
        }
    }

    private int _repeatTokenIndex = -1;

    public LuaSyntaxToken Repeat => Tree.GetElement<LuaSyntaxToken>(_repeatTokenIndex)!;

    private int _blockIndex = -1;

    public LuaBlockSyntax? Block => Tree.GetElement<LuaBlockSyntax>(_blockIndex);

    private int _untilTokenIndex = -1;

    public LuaSyntaxToken? Until => Tree.GetElement<LuaSyntaxToken>(_untilTokenIndex);

    private int _conditionIndex = -1;

    public LuaExprSyntax? Condition => Tree.GetElement<LuaExprSyntax>(_conditionIndex);
}

public class LuaCallStatSyntax: LuaStatSyntax
{
    public LuaCallStatSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.Kind == LuaSyntaxKind.CallExpr)
            {
                _exprIndex = it.Index;
                break;
            }
        }
    }

    private int _exprIndex = -1;

    public LuaCallExprSyntax? CallExpr => Tree.GetElement<LuaCallExprSyntax>(_exprIndex);
}

public class LuaEmptyStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree);

public class LuaUnknownStatSyntax(int index, LuaSyntaxTree tree) : LuaStatSyntax(index, tree);
