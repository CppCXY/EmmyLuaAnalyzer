using System.Text;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaSourceSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();
}

public class LuaBlockSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public IEnumerable<LuaStatSyntax> StatList => ChildrenElement<LuaStatSyntax>();

    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(this) ?? [];
}

public class LuaParamDefSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public bool IsVarArgs => FirstChild<LuaDotsToken>() != null;
}

public class LuaParamListSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public IEnumerable<LuaParamDefSyntax> Params => ChildrenElement<LuaParamDefSyntax>();

    public bool HasVarArgs => Params.LastOrDefault()?.IsVarArgs == true;
}

public class LuaAttributeSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

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
    public LuaAttributeSyntax? Attribute => FirstChild<LuaAttributeSyntax>();

    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaCallArgListSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public IEnumerable<LuaExprSyntax> ArgList => ChildrenElement<LuaExprSyntax>();

    public bool IsSingleArgCall => FirstChildToken(LuaTokenKind.TkLeftParen) != null;

    public LuaExprSyntax? SingleArg => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken? LeftParen => FirstChildToken(LuaTokenKind.TkLeftParen);

    public LuaSyntaxToken? RightParen => FirstChildToken(LuaTokenKind.TkRightParen);
}

public class LuaDescriptionSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public IEnumerable<LuaSyntaxToken> Details => ChildTokens(LuaTokenKind.TkDocDetail);

    public string CommentText
    {
        get
        {
            var sb = new StringBuilder();
            foreach (var token in ChildrenWithTokens)
            {
                if (token is LuaSyntaxToken { Kind: LuaTokenKind.TkDocDetail, RepresentText: { } text })
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
                else if (token is LuaSyntaxToken { Kind: LuaTokenKind.TkDocContinue, Range.Length: { } length })
                {
                    if (length > 3)
                    {
                        sb.Append(' ', length - 3);
                    }
                }
                else if (token is LuaSyntaxToken { Kind: LuaTokenKind.TkEndOfLine })
                {
                    sb.Append('\n');
                }
            }

            return sb.ToString();
        }
    }
}

public class LuaDocFieldSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public VisibilityKind Visibility
    {
        get
        {
            var tk = FirstChildToken(LuaTokenKind.TkTagVisibility);
            return tk == null ? VisibilityKind.Public : VisibilityKindHelper.ToVisibilityKind(tk.Text);
        }
    }

    public bool IsNameField => FirstChildToken(LuaTokenKind.TkName) != null;

    public bool IsStringField => FirstChildToken(LuaTokenKind.TkString) != null;

    public bool IsIntegerField => FirstChildToken(LuaTokenKind.TkInt) != null;

    public bool IsTypeField =>
        ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkRightBracket).FirstOrDefault() != null;

    public LuaNameToken? NameField => FirstChild<LuaNameToken>();

    public LuaStringToken? StringField => FirstChild<LuaStringToken>();

    public LuaIntegerToken? IntegerField => FirstChild<LuaIntegerToken>();

    public LuaDocTypeSyntax? TypeField
    {
        get
        {
            var start = ChildStartIndex;
            if (start == -1)
            {
                return null;
            }

            var finish = ChildFinishIndex;
            for (var i = start; i <= finish; i++)
            {
                if (Tree.GetTokenKind(i) == LuaTokenKind.TkLeftBracket)
                {
                    if (i + 1 < finish)
                    {
                        var element = Tree.GetElement(i + 1);
                        if (element is LuaDocTypeSyntax typeSyntax)
                        {
                            return typeSyntax;
                        }
                    }

                    break;
                }
            }

            return null;
        }
    }

    public bool Nullable => FirstChildToken(LuaTokenKind.TkNullable) != null;

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

    public LuaDocTypeSyntax? Type => IsTypeField
        ? ChildrenElement<LuaDocTypeSyntax>().LastOrDefault()
        : FirstChild<LuaDocTypeSyntax>();

    public LuaDescriptionSyntax? Description => FirstChild<LuaDescriptionSyntax>();
}

public class LuaDocBodySyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public IEnumerable<LuaDocFieldSyntax> FieldList => ChildrenElement<LuaDocFieldSyntax>();
}

public class LuaDocVersionSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public RequiredVersionAction Action
    {
        get
        {
            var tk = FirstChildToken();
            return tk?.Kind switch
            {
                LuaTokenKind.TkGt => RequiredVersionAction.Greater,
                LuaTokenKind.TkGe => RequiredVersionAction.GreaterOrEqual,
                LuaTokenKind.TkLt => RequiredVersionAction.Less,
                LuaTokenKind.TkLe => RequiredVersionAction.LessOrEqual,
                _ => RequiredVersionAction.Equal
            };
        }
    }

    public LuaSyntaxToken? Version => FirstChildToken(LuaTokenKind.TkName);

    public LuaVersionNumberToken? VersionNumber => FirstChild<LuaVersionNumberToken>();
}
