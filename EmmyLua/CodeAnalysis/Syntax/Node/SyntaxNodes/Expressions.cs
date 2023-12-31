﻿using System.Text;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxNode(greenNode, tree, parent)
{
    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(this) ?? Enumerable.Empty<LuaCommentSyntax>();

    private string? _accessPath = null;

    public string AccessPath
    {
        get
        {
            if (_accessPath != null)
            {
                return _accessPath;
            }

            var sb = new StringBuilder();
            var expr = this;
            while (true)
            {
                switch (expr)
                {
                    case LuaIndexExprSyntax indexExpr:
                    {
                        if (indexExpr.IsDotIndex || indexExpr.IsColonIndex)
                        {
                            sb.Insert(0, '.');
                            sb.Insert(0, indexExpr.DotOrColonIndexName!.Text);
                        }

                        expr = indexExpr.PrefixExpr;
                        break;
                    }
                    case LuaCallExprSyntax callExpr:
                    {
                        expr = callExpr.PrefixExpr;
                        break;
                    }
                    case LuaNameExprSyntax nameExpr:
                    {
                        sb.Insert(0, nameExpr.Name!.Text);
                        _accessPath = sb.ToString();
                        return _accessPath;
                    }
                    default:
                    {
                        _accessPath = sb.ToString();
                        return _accessPath;
                    }
                }
            }
        }
    }
}

public class LuaNameExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaExprSyntax(greenNode, tree, parent)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaCallExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaExprSyntax(greenNode, tree, parent)
{
    public LuaCallArgListSyntax? ArgList => FirstChild<LuaCallArgListSyntax>();

    public LuaExprSyntax? PrefixExpr => FirstChild<LuaExprSyntax>();
}

public class LuaBinaryExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaExprSyntax(greenNode, tree, parent)
{
    public LuaExprSyntax LeftExpr => FirstChild<LuaExprSyntax>()!;

    public OperatorKind.BinaryOperator Operator
    {
        get
        {
            var tk = FirstChildToken(it => OperatorKind.ToBinaryOperator(it) != OperatorKind.BinaryOperator.OpNop);
            return tk != null ? OperatorKind.ToBinaryOperator(tk.Kind) : OperatorKind.BinaryOperator.OpNop;
        }
    }

    public LuaExprSyntax? RightExpr => ChildNodes<LuaExprSyntax>().Skip(1).FirstOrDefault();
}

public class LuaUnaryExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaExprSyntax(greenNode, tree, parent)
{
    public OperatorKind.UnaryOperator Operator
    {
        get
        {
            var tk = FirstChildToken(it => OperatorKind.ToUnaryOperator(it) != OperatorKind.UnaryOperator.OpNop);
            return tk != null ? OperatorKind.ToUnaryOperator(tk.Kind) : OperatorKind.UnaryOperator.OpNop;
        }
    }

    public LuaExprSyntax? Expression => FirstChild<LuaExprSyntax>();
}

public class LuaTableExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaExprSyntax(greenNode, tree, parent)
{
    public IEnumerable<LuaTableFieldSyntax> FieldList => ChildNodes<LuaTableFieldSyntax>();

    public string UniqueId => $"table:{Green.Range.StartOffset}";
}

public class LuaTableFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxNode(greenNode, tree, parent)
{
    public bool IsExprKey => ChildNodes<LuaExprSyntax>().Count() == 2;

    public bool IsNameKey => FirstChild<LuaNameToken>() != null;

    public bool IsNumberKey => FirstChild<LuaNumberToken>() != null;

    public bool IsStringKey => FirstChild<LuaStringToken>() != null;

    public bool IsValue => FirstChildToken(LuaTokenKind.TkAssign) == null;

    public LuaExprSyntax? ExprKey => FirstChild<LuaExprSyntax>();

    public LuaNameToken? NameKey => FirstChild<LuaNameToken>();

    public LuaNumberToken? NumberKey => FirstChild<LuaNumberToken>();

    public LuaStringToken? StringKey => FirstChild<LuaStringToken>();

    public LuaExprSyntax? Value => ChildNodes<LuaExprSyntax>().Skip(1).FirstOrDefault();

    public LuaTableExprSyntax? ParentTable => Parent as LuaTableExprSyntax;

    public string? Name
    {
        get
        {
            if (NameKey is {} nameKey)
            {
                return nameKey.RepresentText;
            }
            else if (NumberKey is LuaIntegerToken integerToken)
            {
                return $"[{integerToken.Value}]";
            }
            else if (NumberKey is { } number)
            {
                return $"[{number}]";
            }
            else if (StringKey is {} stringToken)
            {
                return stringToken.Value;
            }

            return null;
        }
    }

    public LuaSyntaxElement? KeyElement
    {
        get
        {
            if (NameKey is {} nameKey)
            {
                return nameKey;
            }
            else if (NumberKey is { } number)
            {
                return number;
            }
            else if (StringKey is {} stringToken)
            {
                return stringToken;
            }

            return null;
        }
    }
}

public class LuaClosureExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaExprSyntax(greenNode, tree, parent), IFuncBodyOwner
{
    public LuaFuncBodySyntax? FuncBody => FirstChild<LuaFuncBodySyntax>();
}

public class LuaLiteralExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaExprSyntax(greenNode, tree, parent)
{
    public LuaSyntaxToken Literal => FirstChildToken()!;
}

public class LuaParenExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaExprSyntax(greenNode, tree, parent)
{
    public LuaSyntaxToken LeftParen => FirstChildToken(LuaTokenKind.TkLeftParen)!;

    public LuaExprSyntax? Inner => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken? RightParen => FirstChildToken(LuaTokenKind.TkRightParen);
}

public class LuaIndexExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaExprSyntax(greenNode, tree, parent)
{
    public bool IsDotIndex => FirstChildToken(LuaTokenKind.TkDot) != null;

    public bool IsColonIndex => FirstChildToken(LuaTokenKind.TkColon) != null;

    public bool IsKeyIndex => FirstChildToken(LuaTokenKind.TkLeftBracket) != null;

    public LuaNameToken? DotOrColonIndexName => FirstChild<LuaNameToken>();

    public LuaExprSyntax? IndexKeyExpr => ChildNodeAfterToken<LuaExprSyntax>(LuaTokenKind.TkLeftBracket);

    public LuaExprSyntax? PrefixExpr => FirstChild<LuaExprSyntax>();

    public string? Name
    {
        get
        {
            if (DotOrColonIndexName != null)
            {
                return DotOrColonIndexName.RepresentText;
            }
            else if (IndexKeyExpr is LuaLiteralExprSyntax literal)
            {
                if (literal.Literal is LuaStringToken stringToken)
                {
                    return  stringToken.Value;
                }
                else if (literal.Literal is LuaIntegerToken luaIntegerToken)
                {
                    return $"[{luaIntegerToken.Value}]";
                }
                else
                {
                    return literal.Literal.RepresentText;
                }
            }

            return null;
        }
    }

    public LuaSyntaxElement? KeyElement
    {
        get
        {
            if (DotOrColonIndexName != null)
            {
                return DotOrColonIndexName;
            }

            return IndexKeyExpr;
        }
    }
}
