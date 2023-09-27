using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaExprSyntax : LuaSyntaxNode
{
    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(this) ?? Enumerable.Empty<LuaCommentSyntax>();

    public LuaExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaNameSyntax : LuaExprSyntax
{
    public LuaSyntaxToken Name => FirstChildToken(LuaTokenKind.TkName)!;

    public LuaNameSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaVarDefSyntax : LuaExprSyntax
{
    public LuaSuffixExprSyntax? Expression => FirstChild<LuaSuffixExprSyntax>();

    public LuaSyntaxToken? Name
    {
        get
        {
            var expr = Expression;
            if (expr is null)
            {
                return null;
            }

            return expr.ChildNodes<LuaNameSyntax>().LastOrDefault()?.Name;
        }
    }

    public LuaVarDefSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaCallExprSyntax : LuaExprSyntax
{
    public bool IsSingleArgCall => FirstChildToken(LuaTokenKind.TkLeftParen) != null;

    public LuaExprSyntax? SingleArg => FirstChild<LuaExprSyntax>();

    public IEnumerable<LuaExprSyntax> ArgList => ChildNodes<LuaExprSyntax>();

    public LuaExprSyntax? ParentExpr => PrevOfType<LuaExprSyntax>();

    public LuaCallExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaBinaryExprSyntax : LuaExprSyntax
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

    public LuaBinaryExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaUnaryExprSyntax : LuaExprSyntax
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

    public LuaUnaryExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaTableExprSyntax : LuaExprSyntax
{
    public IEnumerable<LuaTableFieldSyntax> FieldList => ChildNodes<LuaTableFieldSyntax>();

    public LuaTableExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaTableFieldSyntax : LuaSyntaxNode
{
    public bool IsExprKey => ChildNodes<LuaExprSyntax>().Count() == 2;

    public bool IsNameKey => FirstChildToken(LuaTokenKind.TkName) != null;

    public bool IsNumberKey => FirstChildToken(LuaTokenKind.TkNumber) != null;

    public bool IsStringKey => FirstChildToken(LuaTokenKind.TkString) != null;

    public bool IsValue => FirstChildToken(LuaTokenKind.TkAssign) == null;

    public LuaExprSyntax? ExprKey => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken? NameKey => FirstChildToken(LuaTokenKind.TkName);

    public LuaSyntaxToken? NumberKey => FirstChildToken(LuaTokenKind.TkNumber);

    public LuaSyntaxToken? StringKey => FirstChildToken(LuaTokenKind.TkString);

    public LuaExprSyntax? Value => ChildNodes<LuaExprSyntax>().Skip(1).FirstOrDefault();

    public LuaTableFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaClosureExprSyntax : LuaExprSyntax
{
    public LuaSyntaxToken Function => FirstChildToken(LuaTokenKind.TkFunction)!;

    public LuaParamListSyntax? ParamList => FirstChild<LuaParamListSyntax>();

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

    public LuaSyntaxToken? End => FirstChildToken(LuaTokenKind.TkEnd);

    public LuaClosureExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaLiteralExprSyntax : LuaExprSyntax
{
    public LuaSyntaxToken Literal => FirstChildToken()!;

    public LuaLiteralExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaParenExprSyntax : LuaExprSyntax
{
    public LuaSyntaxToken LeftParen => FirstChildToken(LuaTokenKind.TkLeftParen)!;

    public LuaExprSyntax? Inner => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken? RightParen => FirstChildToken(LuaTokenKind.TkRightParen);

    public LuaParenExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaIndexExprSyntax : LuaExprSyntax
{
    public bool IsDotIndex => FirstChildToken(LuaTokenKind.TkDot) != null;

    public bool IsColonIndex => FirstChildToken(LuaTokenKind.TkColon) != null;

    public bool IsKeyIndex => FirstChildToken(LuaTokenKind.TkLeftBracket) != null;

    public LuaSyntaxToken? DotOrColonIndexName => FirstChildToken(LuaTokenKind.TkName);

    public LuaExprSyntax? IndexKeyExpr => FirstChild<LuaExprSyntax>();

    public LuaExprSyntax? ParentExpr => PrevOfType<LuaExprSyntax>();

    public LuaSyntaxToken? Name
    {
        get
        {
            if (IsDotIndex && IsColonIndex)
            {
                return DotOrColonIndexName;
            }
            else if (IndexKeyExpr is LuaLiteralExprSyntax
                     {
                         Literal: { Kind: LuaTokenKind.TkString or LuaTokenKind.TkLongString }
                     } literalExpr)
            {
                return literalExpr.Literal;
            }
            else
            {
                return null;
            }
        }
    }

    public LuaIndexExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaSuffixExprSyntax : LuaExprSyntax
{
    public LuaExprSyntax? Prefix => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken Dot => FirstChildToken(LuaTokenKind.TkDot)!;

    public LuaNameSyntax? Name => FirstChild<LuaNameSyntax>();

    public LuaCallExprSyntax? Call => FirstChild<LuaCallExprSyntax>();

    public LuaIndexExprSyntax? Index => FirstChild<LuaIndexExprSyntax>();

    public LuaSuffixExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaRequireExprSyntax : LuaExprSyntax
{
    public LuaSyntaxToken Name => FirstChildToken(LuaTokenKind.TkName)!;

    public string ModulePath
    {
        get
        {
            var callExpr = FirstChild<LuaCallExprSyntax>();
            if (callExpr is null)
            {
                return string.Empty;
            }

            if (callExpr.IsSingleArgCall)
            {
                var path = callExpr.SingleArg?.FirstChildToken();
                if (path != null)
                {
                    return path.RepresentText;
                }
            }
            else
            {
                var firstExpr = callExpr.ArgList.FirstOrDefault();
                if (firstExpr is LuaLiteralExprSyntax
                    {
                        Literal.Kind: LuaTokenKind.TkString or LuaTokenKind.TkLongString
                    } literalExpr)
                {
                    return literalExpr.Literal.RepresentText;
                }
            }

            return string.Empty;
        }
    }

    public LuaRequireExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}
