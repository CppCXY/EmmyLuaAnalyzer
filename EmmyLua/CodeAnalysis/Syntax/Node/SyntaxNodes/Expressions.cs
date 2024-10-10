using System.Diagnostics;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaExprSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public static bool CanCast(LuaSyntaxKind kind) => kind is >= LuaSyntaxKind.ParenExpr and <= LuaSyntaxKind.NameExpr;
}

public class LuaNameExprSyntax : LuaExprSyntax
{
    public LuaNameExprSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind is LuaTokenKind.TkName)
            {
                _nameIndex = it.Index;
                break;
            }
        }
    }

    private int _nameIndex = -1;

    public LuaNameToken? Name => Tree.GetElement<LuaNameToken>(_nameIndex);
}

public class LuaCallExprSyntax : LuaExprSyntax
{
    public LuaCallExprSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.Kind is LuaSyntaxKind.CallArgList)
            {
                _argListIndex = it.Index;
            }
            else if (it.Kind is LuaSyntaxKind.IndexExpr or LuaSyntaxKind.NameExpr)
            {
                _prefixExprIndex = it.Index;
            }
        }
    }

    private int _argListIndex = -1;

    public LuaCallArgListSyntax? ArgList => Tree.GetElement<LuaCallArgListSyntax>(_argListIndex);

    private int _prefixExprIndex = -1;

    public LuaExprSyntax? PrefixExpr => Tree.GetElement<LuaExprSyntax>(_prefixExprIndex);

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

public class LuaBinaryExprSyntax : LuaExprSyntax
{
    public LuaBinaryExprSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (CanCast(it.Kind))
            {
                if (_leftExprIndex == -1)
                {
                    _leftExprIndex = it.Index;
                }
                else
                {
                    _rightExprIndex = it.Index;
                    break;
                }
            }
            else if (OperatorKind.ToBinaryOperator(it.TokenKind) is { } op && op != OperatorKind.BinaryOperator.OpNop)
            {
                Operator = op;
            }
        }
    }

    private int _leftExprIndex = -1;

    public LuaExprSyntax LeftExpr => Tree.GetElement<LuaExprSyntax>(_leftExprIndex)!;

    public OperatorKind.BinaryOperator Operator { get; } = OperatorKind.BinaryOperator.OpNop;

    private int _rightExprIndex = -1;

    public LuaExprSyntax? RightExpr => Tree.GetElement<LuaExprSyntax>(_rightExprIndex);
}

public class LuaUnaryExprSyntax : LuaExprSyntax
{
    public LuaUnaryExprSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (OperatorKind.ToUnaryOperator(it.TokenKind) is { } op && op != OperatorKind.UnaryOperator.OpNop)
            {
                Operator = op;
            }
            else if (CanCast(it.Kind))
            {
                _exprIndex = it.Index;
                break;
            }
        }
    }

    public OperatorKind.UnaryOperator Operator { get; } = OperatorKind.UnaryOperator.OpNop;

    private int _exprIndex = -1;

    public LuaExprSyntax? Expression => Tree.GetElement<LuaExprSyntax>(_exprIndex);
}

public class LuaTableExprSyntax : LuaExprSyntax
{
    public enum TableState
    {
        Empty,
        Array,
        Dictionary,
        Mixed
    }

    public LuaTableExprSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.Kind is LuaSyntaxKind.TableFieldAssign or LuaSyntaxKind.TableFieldValue)
            {
                _fieldListIndex.Add(it.Index);
                switch (_tableState)
                {
                    case TableState.Empty:
                        _tableState = it.Kind == LuaSyntaxKind.TableFieldAssign
                            ? TableState.Dictionary
                            : TableState.Array;
                        break;
                    case TableState.Array when it.Kind == LuaSyntaxKind.TableFieldAssign:
                    case TableState.Dictionary when it.Kind == LuaSyntaxKind.TableFieldValue:
                        _tableState = TableState.Mixed;
                        break;
                }
            }
        }
    }

    private TableState _tableState = TableState.Empty;

    public bool IsArray => _tableState == TableState.Array;

    public bool IsDictionary => _tableState == TableState.Dictionary;

    public bool IsMixed => _tableState == TableState.Mixed;

    public bool IsEmpty => _tableState == TableState.Empty;

    private List<int> _fieldListIndex = [];

    public IEnumerable<LuaTableFieldSyntax> FieldList => Tree.GetElements<LuaTableFieldSyntax>(_fieldListIndex);
}

public class LuaTableFieldSyntax : LuaSyntaxNode, ICommentOwner
{
    public enum TableFieldState
    {
        UnknownKeyValue,
        NameKeyValue,
        StringKeyValue,
        IntegerKeyValue,
        ExprKeyValue,
        Value
    }

    public LuaTableFieldSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        _tableFieldState = Kind switch
        {
            LuaSyntaxKind.TableFieldAssign => TableFieldState.UnknownKeyValue,
            LuaSyntaxKind.TableFieldValue => TableFieldState.Value,
            _ => throw new UnreachableException()
        };

        foreach (var it in Iter.Children)
        {
            if (LuaExprSyntax.CanCast(it.Kind))
            {
                if (_tableFieldState == TableFieldState.UnknownKeyValue)
                {
                    _tableFieldState = TableFieldState.ExprKeyValue;
                    _keyIndex = it.Index;
                }
                else
                {
                    _valueIndex = it.Index;
                }
            }
            else
            {
                switch (it.TokenKind)
                {
                    case LuaTokenKind.TkString:
                    case LuaTokenKind.TkLongString:
                    {
                        _tableFieldState = TableFieldState.StringKeyValue;
                        _keyIndex = it.Index;
                        break;
                    }
                    case LuaTokenKind.TkName:
                    {
                        _tableFieldState = TableFieldState.NameKeyValue;
                        _keyIndex = it.Index;
                        break;
                    }
                    case LuaTokenKind.TkInt:
                    {
                        _tableFieldState = TableFieldState.IntegerKeyValue;
                        _keyIndex = it.Index;
                        break;
                    }
                }
            }
        }
    }

    private TableFieldState _tableFieldState;

    public bool IsExprKey => _tableFieldState == TableFieldState.ExprKeyValue;

    public bool IsNameKey => _tableFieldState == TableFieldState.NameKeyValue;

    public bool IsIntegerKey => _tableFieldState == TableFieldState.IntegerKeyValue;

    public bool IsStringKey => _tableFieldState == TableFieldState.StringKeyValue;

    public bool IsValue => _tableFieldState == TableFieldState.Value;

    private int _keyIndex = -1;

    public LuaExprSyntax? ExprKey => Tree.GetElement<LuaExprSyntax>(_keyIndex);

    public LuaNameToken? NameKey => Tree.GetElement<LuaNameToken>(_keyIndex);

    public LuaIntegerToken? IntegerKey => Tree.GetElement<LuaIntegerToken>(_keyIndex);

    public LuaStringToken? StringKey => Tree.GetElement<LuaStringToken>(_keyIndex);

    private int _valueIndex = -1;

    public LuaExprSyntax? Value => Tree.GetElement<LuaExprSyntax>(_valueIndex);

    public LuaTableExprSyntax? ParentTable => Iter.Parent.ToNode<LuaTableExprSyntax>();

    public string? Name
    {
        get
        {
            switch (_tableFieldState)
            {
                case TableFieldState.NameKeyValue:
                    return NameKey?.RepresentText;
                case TableFieldState.StringKeyValue:
                    return StringKey?.Value;
                case TableFieldState.IntegerKeyValue:
                    return $"[{IntegerKey?.Value}]";
                default:
                    return null;
            }
        }
    }

    public LuaSyntaxElement? KeyElement
    {
        get
        {
            switch (_tableFieldState)
            {
                case TableFieldState.NameKeyValue:
                    return NameKey;
                case TableFieldState.StringKeyValue:
                    return StringKey;
                case TableFieldState.IntegerKeyValue:
                    return IntegerKey;
                case TableFieldState.ExprKeyValue:
                    return ExprKey;
                default:
                    return null;
            }
        }
    }

    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(this) ?? [];
}

public class LuaClosureExprSyntax : LuaExprSyntax
{
    public LuaClosureExprSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.Kind is LuaSyntaxKind.ParamList)
            {
                _paramListIndex = it.Index;
            }
            else if (it.Kind is LuaSyntaxKind.Block)
            {
                _blockIndex = it.Index;
            }
        }
    }

    private int _paramListIndex = -1;

    public LuaParamListSyntax? ParamList => Tree.GetElement<LuaParamListSyntax>(_paramListIndex);

    private int _blockIndex = -1;

    public LuaBlockSyntax? Block => Tree.GetElement<LuaBlockSyntax>(_blockIndex);
}

public class LuaLiteralExprSyntax : LuaExprSyntax
{
    public enum LiteralType
    {
        None,
        Nil,
        True,
        False,
        Integer,
        Number,
        String
    }

    public LuaLiteralExprSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind is LuaTokenKind.TkNil)
            {
                Type = LiteralType.Nil;
                _literalIndex = it.Index;
            }
            else if (it.TokenKind is LuaTokenKind.TkTrue)
            {
                Type = LiteralType.True;
                _literalIndex = it.Index;
            }
            else if (it.TokenKind is LuaTokenKind.TkFalse)
            {
                Type = LiteralType.False;
                _literalIndex = it.Index;
            }
            else if (it.TokenKind is LuaTokenKind.TkInt)
            {
                Type = LiteralType.Integer;
                _literalIndex = it.Index;
            }
            else if (it.TokenKind is LuaTokenKind.TkFloat or LuaTokenKind.TkComplex)
            {
                Type = LiteralType.Number;
                _literalIndex = it.Index;
            }
            else if (it.TokenKind is LuaTokenKind.TkString or LuaTokenKind.TkLongString)
            {
                Type = LiteralType.String;
                _literalIndex = it.Index;
            }
        }
    }

    public LiteralType Type { get; } = LiteralType.None;

    public bool IsNil => Type == LiteralType.Nil;

    public bool IsTrue => Type == LiteralType.True;

    public bool IsFalse => Type == LiteralType.False;

    public bool IsInteger => Type == LiteralType.Integer;

    public bool IsNumber => Type == LiteralType.Number;

    public bool IsString => Type == LiteralType.String;

    private int _literalIndex = -1;

    public LuaSyntaxToken Literal => Tree.GetElement<LuaSyntaxToken>(_literalIndex)!;
}

public class LuaParenExprSyntax(int index, LuaSyntaxTree tree) : LuaExprSyntax(index, tree)
{
    public LuaSyntaxToken LeftParen => Iter.FirstChildToken(LuaTokenKind.TkLeftParen).ToToken<LuaSyntaxToken>()!;

    public LuaExprSyntax? Inner => Iter.FirstChildNode(LuaExprSyntax.CanCast).ToNode<LuaExprSyntax>();

    public LuaSyntaxToken? RightParen => Iter.FirstChildToken(LuaTokenKind.TkRightParen).ToToken<LuaSyntaxToken>();
}

public class LuaIndexExprSyntax : LuaExprSyntax
{
    enum IndexState
    {
        None,
        Dot,
        Colon,
        Key
    }

    public LuaIndexExprSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (CanCast(it.Kind))
            {
                if (_prefixExprIndex == -1)
                {
                    _prefixExprIndex = it.Index;
                }
                else
                {
                    _indexKeyIndex = it.Index;
                    break;
                }
            }
            else
            {
                switch (it.TokenKind)
                {
                    case LuaTokenKind.TkDot:
                        _indexState = IndexState.Dot;
                        break;
                    case LuaTokenKind.TkColon:
                        _indexState = IndexState.Colon;
                        break;
                    case LuaTokenKind.TkLeftBracket:
                        _indexState = IndexState.Key;
                        break;
                    case LuaTokenKind.TkName:
                        _indexKeyIndex = it.Index;
                        break;
                }
            }
        }
    }

    private IndexState _indexState = IndexState.None;

    public bool IsDotIndex => _indexState == IndexState.Dot;

    public LuaSyntaxToken? Dot => Iter.FirstChildToken(LuaTokenKind.TkDot).ToToken<LuaSyntaxToken>();

    public bool IsColonIndex => _indexState == IndexState.Colon;

    public bool IsKeyIndex => _indexState == IndexState.Key;

    private int _indexKeyIndex = -1;

    public LuaNameToken? DotOrColonIndexName => Tree.GetElement<LuaNameToken>(_indexKeyIndex);

    public LuaExprSyntax? IndexKeyExpr => Tree.GetElement<LuaExprSyntax>(_indexKeyIndex);

    private int _prefixExprIndex = -1;

    public LuaExprSyntax? PrefixExpr => Tree.GetElement<LuaExprSyntax>(_prefixExprIndex);

    public string? Name
    {
        get
        {
            if (IsKeyIndex)
            {
                if (IndexKeyExpr is LuaLiteralExprSyntax literal)
                {
                    if (literal is { IsString: true, Literal: LuaStringToken { Value: { } value } })
                    {
                        return value;
                    }
                    else if (literal is { IsInteger: true, Literal: LuaIntegerToken { Value: { } value2 } })
                    {
                        return $"[{value2}]";
                    }
                }
            }
            else
            {
                return DotOrColonIndexName?.RepresentText;
            }

            return null;
        }
    }

    public LuaSyntaxElement? KeyElement => IsKeyIndex ? IndexKeyExpr : DotOrColonIndexName;
}
