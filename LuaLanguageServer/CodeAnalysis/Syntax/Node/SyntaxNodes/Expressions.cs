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

public class LuaNameExprSyntax : LuaExprSyntax
{
    public LuaSyntaxToken Name => FirstChildToken(LuaTokenKind.TkName)!;

    public LuaNameExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaCallExprSyntax : LuaExprSyntax
{
    public LuaCallArgListSyntax? ArgList => FirstChild<LuaCallArgListSyntax>();

    public LuaExprSyntax? PrefixExpr => FirstChild<LuaExprSyntax>();

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
    public LuaParamListSyntax? ParamList => FirstChild<LuaParamListSyntax>();

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();

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

    public LuaExprSyntax? IndexKeyExpr => ChildNodeAfterToken<LuaExprSyntax>(LuaTokenKind.TkLeftBracket);

    public LuaExprSyntax? PrefixExpr => FirstChild<LuaExprSyntax>();

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
                         Literal: {Kind: LuaTokenKind.TkString or LuaTokenKind.TkLongString}
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

public class LuaRequireExprSyntax : LuaExprSyntax
{
    public LuaSyntaxToken Name => FirstChildToken(LuaTokenKind.TkName)!;

    public string ModulePath
    {
        get
        {
            var argList = FirstChild<LuaCallArgListSyntax>();
            if (argList is null)
            {
                return string.Empty;
            }

            var firstArg = argList.ArgList.FirstOrDefault();
            if (firstArg is LuaLiteralExprSyntax
                {
                    Literal.Kind: LuaTokenKind.TkString or LuaTokenKind.TkLongString
                } literalExpr)
            {
                return literalExpr.Literal.RepresentText;
            }


            return string.Empty;
        }
    }

    public LuaRequireExprSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}
