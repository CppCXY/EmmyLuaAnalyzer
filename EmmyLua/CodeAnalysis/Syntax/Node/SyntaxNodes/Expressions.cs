using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Syntax.Tree.Green;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxNode(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(this) ?? [];
}

public class LuaNameExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaExprSyntax(greenNode, tree, parent, startOffset)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaCallExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaExprSyntax(greenNode, tree, parent, startOffset)
{
    public LuaCallArgListSyntax? ArgList => FirstChild<LuaCallArgListSyntax>();

    public LuaExprSyntax? PrefixExpr => FirstChild<LuaExprSyntax>();

    public string Name
    {
        get
        {
            var expr = PrefixExpr;
            if (expr is LuaIndexExprSyntax indexExpr)
            {
                return indexExpr?.Name ?? string.Empty;
            }

            if (expr is LuaNameExprSyntax nameExpr)
            {
                return nameExpr.Name?.RepresentText ?? string.Empty;
            }

            return string.Empty;
        }
    }
}

public class LuaBinaryExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaExprSyntax(greenNode, tree, parent, startOffset)
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

public class LuaUnaryExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaExprSyntax(greenNode, tree, parent, startOffset)
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

public class LuaTableExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaExprSyntax(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaTableFieldSyntax> FieldList => ChildNodes<LuaTableFieldSyntax>();
}

public class LuaTableFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxNode(greenNode, tree, parent, startOffset)
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

    public LuaExprSyntax? Value => ChildNodes<LuaExprSyntax>().LastOrDefault();

    public LuaTableExprSyntax? ParentTable => Parent as LuaTableExprSyntax;

    public string? Name
    {
        get
        {
            // optimize
            foreach (var element in ChildrenElements)
            {
                switch (element)
                {
                    case LuaNameToken nameToken:
                    {
                        return nameToken.RepresentText;
                    }
                    case LuaIntegerToken integerToken:
                    {
                        return $"[{integerToken.Value}]";
                    }
                    case LuaStringToken stringToken:
                    {
                        return stringToken.Value;
                    }
                    case LuaSyntaxToken { Kind: LuaTokenKind.TkEq }:
                    {
                        goto endLoop;
                    }
                }
            }
            endLoop:
            return null;
        }
    }

    public LuaSyntaxElement? KeyElement
    {
        get
        {
            foreach (var element in ChildrenElements)
            {
                switch (element)
                {
                    case LuaNameToken nameToken:
                    {
                        return nameToken;
                    }
                    case LuaIntegerToken integerToken:
                    {
                        return integerToken;
                    }
                    case LuaStringToken stringToken:
                    {
                        return stringToken;
                    }
                    case LuaExprSyntax exprSyntax:
                    {
                        return exprSyntax;
                    }
                    case LuaSyntaxToken { Kind: LuaTokenKind.TkEq }:
                    {
                        goto endLoop;
                    }
                }
            }
            endLoop:
            return null;
        }
    }

    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(this) ?? [];
}

public class LuaClosureExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaExprSyntax(greenNode, tree, parent, startOffset)
{
    public LuaParamListSyntax? ParamList => FirstChild<LuaParamListSyntax>();

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();
}

public class LuaLiteralExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaExprSyntax(greenNode, tree, parent, startOffset)
{
    public LuaSyntaxToken Literal => FirstChildToken()!;
}

public class LuaParenExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaExprSyntax(greenNode, tree, parent, startOffset)
{
    public LuaSyntaxToken LeftParen => FirstChildToken(LuaTokenKind.TkLeftParen)!;

    public LuaExprSyntax? Inner => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken? RightParen => FirstChildToken(LuaTokenKind.TkRightParen);
}

public class LuaIndexExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaExprSyntax(greenNode, tree, parent, startOffset)
{
    public bool IsDotIndex => FirstChildToken(LuaTokenKind.TkDot) != null;

    public LuaSyntaxToken? Dot => FirstChildToken(LuaTokenKind.TkDot);

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

            if (IndexKeyExpr is LuaLiteralExprSyntax literal)
            {
                if (literal.Literal is LuaStringToken stringToken)
                {
                    return stringToken.Value;
                }

                if (literal.Literal is LuaIntegerToken luaIntegerToken)
                {
                    return $"[{luaIntegerToken.Value}]";
                }

                return literal.Literal.RepresentText;
            }

            return null;
        }
    }

    public LuaSyntaxElement KeyElement
    {
        get
        {
            if (DotOrColonIndexName != null)
            {
                return DotOrColonIndexName;
            }

            return IndexKeyExpr ?? PrefixExpr!;
        }
    }
}
