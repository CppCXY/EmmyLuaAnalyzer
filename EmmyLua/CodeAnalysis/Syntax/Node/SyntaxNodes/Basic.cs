using System.Text;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaSourceSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public LuaBlockSyntax? Block => Iter.FirstChildNode(LuaSyntaxKind.Block).ToNode<LuaBlockSyntax>();
}

public class LuaBlockSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public IEnumerable<LuaStatSyntax> StatList => Iter.ChildrenNodeOfType<LuaStatSyntax>(LuaStatSyntax.CanCast);

    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(this) ?? [];
}

public class LuaParamDefSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public LuaNameToken? Name => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();

    public bool IsVarArgs => Iter.FirstChildToken(LuaTokenKind.TkDots).IsValid;
}

public class LuaParamListSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public IEnumerable<LuaParamDefSyntax> Params => Iter.ChildrenNodeOfType<LuaParamDefSyntax>(LuaSyntaxKind.ParamName);

    public bool HasVarArgs => Params.LastOrDefault()?.IsVarArgs == true;
}

public class LuaAttributeSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public LuaNameToken? Name => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();

    public bool IsConst
    {
        get
        {
            if (Name == null)
            {
                return false;
            }

            return Name.Text is "const";
        }
    }

    public bool IsClose
    {
        get
        {
            if (Name == null)
            {
                return false;
            }

            return Name.Text is "close";
        }
    }
}

public class LuaLocalNameSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public LuaAttributeSyntax? Attribute => Iter.FirstChildNode(LuaSyntaxKind.Attribute).ToNode<LuaAttributeSyntax>();

    public LuaNameToken? Name => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();
}

public class LuaCallArgListSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public IEnumerable<LuaExprSyntax> ArgList => Iter.ChildrenNodeOfType<LuaExprSyntax>(LuaExprSyntax.CanCast);

    public bool IsSingleArgCall => !Iter.FirstChildToken(LuaTokenKind.TkLeftParen).IsValid;

    public LuaExprSyntax? SingleArg => ArgList.FirstOrDefault();

    public LuaSyntaxToken? LeftParen => Iter.FirstChildToken(LuaTokenKind.TkLeftParen).ToToken<LuaSyntaxToken>();

    public LuaSyntaxToken? RightParen => Iter.FirstChildToken(LuaTokenKind.TkRightParen).ToToken<LuaSyntaxToken>();
}

public class LuaDescriptionSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public IEnumerable<LuaSyntaxToken> Details => Iter.ChildrenTokenOfType<LuaSyntaxToken>(LuaTokenKind.TkDocDetail);

    public string CommentText
    {
        get
        {
            var sb = new StringBuilder();
            foreach (var token in Iter.Children)
            {
                if (token.TokenKind == LuaTokenKind.TkDocDetail &&
                    token.ToToken<LuaSyntaxToken>() is { Text: { } text })
                {
                    if (text.StartsWith('@') || text.StartsWith('#'))
                    {
                        sb.Append(text[1..]);
                    }
                    else
                    {
                        sb.Append(text);
                    }
                }
                else if (token is { TokenKind: LuaTokenKind.TkDocContinue, Range.Length: { } length })
                {
                    if (length > 3)
                    {
                        sb.Append(' ', length - 3);
                    }
                }
                else if (token is { TokenKind: LuaTokenKind.TkEndOfLine })
                {
                    sb.Append('\n');
                }
            }

            return sb.ToString();
        }
    }
}

public class LuaDocFieldSyntax : LuaSyntaxNode
{
    public LuaDocFieldSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            var foundLeftBracket = false;
            if (it.TokenKind == LuaTokenKind.TkDocVisibility)
            {
                var token = it.ToToken<LuaSyntaxToken>();
                if (token != null)
                {
                    Visibility = VisibilityKindHelper.ToVisibilityKind(token.Text);
                }
            }
            else if (it.TokenKind == LuaTokenKind.TkDocReadonly)
            {
                ReadOnly = true;
            }
            else if (it.TokenKind == LuaTokenKind.TkName)
            {
                IsNameField = true;
                _fieldIndex = it.Index;
            }
            else if (it.TokenKind == LuaTokenKind.TkString)
            {
                IsStringField = true;
                _fieldIndex = it.Index;
                foundLeftBracket = false;
            }
            else if (it.TokenKind == LuaTokenKind.TkInt)
            {
                IsIntegerField = true;
                _fieldIndex = it.Index;
                foundLeftBracket = false;
            }
            else if (it.TokenKind == LuaTokenKind.TkDocQuestion)
            {
                Nullable = true;
            }
            else if (it.Kind == LuaSyntaxKind.Description)
            {
                _descriptionIndex = it.Index;
            }
            else if (it.TokenKind == LuaTokenKind.TkLeftBracket)
            {
                foundLeftBracket = true;
            }
            else if (LuaDocTypeSyntax.CanCast(it.Kind))
            {
                if (!foundLeftBracket)
                {
                    _typeIndex = it.Index;
                    continue;
                }

                foundLeftBracket = false;
                if (!IsIntegerField && !IsStringField && TypeField is null)
                {
                    _typeIndex = it.Index;
                }
            }
        }
    }

    public VisibilityKind Visibility { get; } = VisibilityKind.Public;

    public bool ReadOnly { get; }

    public bool IsNameField { get; }

    public bool IsStringField { get; }

    public bool IsIntegerField { get; }

    public bool IsTypeField => TypeField != null;

    private int _fieldIndex = -1;

    private int _typeIndex = -1;

    public LuaNameToken? NameField => IsNameField ? Tree.GetElement<LuaNameToken>(_fieldIndex) : null;

    public LuaStringToken? StringField => IsStringField ? Tree.GetElement<LuaStringToken>(_fieldIndex) : null;

    public LuaIntegerToken? IntegerField => IsIntegerField ? Tree.GetElement<LuaIntegerToken>(_fieldIndex) : null;

    public LuaDocTypeSyntax? TypeField => IsTypeField ? Tree.GetElement<LuaDocTypeSyntax>(_typeIndex) : null;

    public LuaDocTypeSyntax? Type => IsTypeField ? TypeField : null;

    public bool Nullable { get; }

    public LuaSyntaxElement? FieldElement
    {
        get
        {
            if (IsNameField)
            {
                return NameField!;
            }

            if (IsStringField)
            {
                return StringField!;
            }

            if (IsIntegerField)
            {
                return IntegerField!;
            }

            return null;
        }
    }

    public string? Name
    {
        get
        {
            if (IsNameField)
            {
                return NameField!.RepresentText;
            }

            if (IsStringField)
            {
                return StringField!.Value;
            }

            if (IsIntegerField)
            {
                return $"[{IntegerField!.Value}]";
            }

            return null;
        }
    }

    private int _descriptionIndex = -1;

    public LuaDescriptionSyntax? Description => Tree.GetElement<LuaDescriptionSyntax>(_descriptionIndex);
}

public class LuaDocBodySyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public IEnumerable<LuaDocFieldSyntax> FieldList =>
        Iter.ChildrenNodeOfType<LuaDocFieldSyntax>(LuaSyntaxKind.DocDetailField);
}

public class LuaDocVersionSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public RequiredVersionAction Action
    {
        get
        {
            var tk = Iter.FirstChildToken();
            return tk.TokenKind switch
            {
                LuaTokenKind.TkGt => RequiredVersionAction.Greater,
                LuaTokenKind.TkGe => RequiredVersionAction.GreaterOrEqual,
                LuaTokenKind.TkLt => RequiredVersionAction.Less,
                LuaTokenKind.TkLe => RequiredVersionAction.LessOrEqual,
                _ => RequiredVersionAction.Equal
            };
        }
    }

    public LuaNameToken? Version => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();

    public LuaVersionNumberToken? VersionNumber =>
        Iter.FirstChildToken(LuaTokenKind.TkVersionNumber).ToToken<LuaVersionNumberToken>();
}
