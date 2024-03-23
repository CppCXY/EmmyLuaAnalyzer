using System.Text;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaSourceSyntax(GreenNode greenNode, LuaSyntaxTree tree) : LuaSyntaxNode(greenNode, tree, null, 0)
{
    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();
}

public class LuaBlockSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxNode(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaStatSyntax> StatList => ChildNodes<LuaStatSyntax>();

    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(this) ?? Enumerable.Empty<LuaCommentSyntax>();
}

public class LuaParamDefSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxNode(greenNode, tree, parent, startOffset)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public bool IsVarArgs => FirstChild<LuaDotsToken>() != null;
}

public class LuaParamListSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxNode(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaParamDefSyntax> Params => ChildNodes<LuaParamDefSyntax>();

    public bool HasVarArgs => Params.LastOrDefault()?.IsVarArgs == true;
}

public class LuaAttributeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxNode(greenNode, tree, parent, startOffset)
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

public class LuaLocalNameSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxNode(greenNode, tree, parent, startOffset)
{
    public LuaAttributeSyntax? Attribute => FirstChild<LuaAttributeSyntax>();

    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaCallArgListSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxNode(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaExprSyntax> ArgList => ChildNodes<LuaExprSyntax>();

    public bool IsSingleArgCall => FirstChildToken(LuaTokenKind.TkLeftParen) != null;

    public LuaExprSyntax? SingleArg => FirstChild<LuaExprSyntax>();

    public LuaSyntaxToken? LeftParen => FirstChildToken(LuaTokenKind.TkLeftParen);

    public LuaSyntaxToken? RightParen => FirstChildToken(LuaTokenKind.TkRightParen);
}

public class LuaDescriptionSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxNode(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaSyntaxToken> Details => ChildTokens(LuaTokenKind.TkDocDetail);

    public string CommentText
    {
        get
        {
            var sb = new StringBuilder();
            foreach (var token in ChildrenWithTokens)
            {
                if (token is LuaSyntaxToken { Kind: LuaTokenKind.TkDocDetail , RepresentText: {} text})
                {
                    sb.Append(text);
                }
                else if(token is LuaSyntaxToken { Kind: LuaTokenKind.TkDocContinue })
                {
                    sb.Append('\n');
                }
            }

            return sb.ToString();
        }
    }
}

public class LuaDocFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxNode(greenNode, tree, parent, startOffset)
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

    public LuaDocTypeSyntax? TypeField =>
        ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkRightBracket).FirstOrDefault();

    public bool Nullable => FirstChildToken(LuaTokenKind.TkNullable) != null;

    public LuaDocTypeSyntax? Type => IsTypeField
        ? ChildNodes<LuaDocTypeSyntax>().LastOrDefault()
        : FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocBodySyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxNode(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaDocFieldSyntax> FieldList => ChildNodes<LuaDocFieldSyntax>();
}
