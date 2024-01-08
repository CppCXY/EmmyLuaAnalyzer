using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaSourceSyntax(GreenNode greenNode, LuaSyntaxTree tree) : LuaSyntaxNode(greenNode, tree, null)
{
    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();
}

public class LuaBlockSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxNode(greenNode, tree, parent)
{
    public IEnumerable<LuaStatSyntax> StatList => ChildNodes<LuaStatSyntax>();

    public IEnumerable<LuaCommentSyntax> Comments =>
        Tree.BinderData?.GetComments(this) ?? Enumerable.Empty<LuaCommentSyntax>();
}

public class LuaParamDefSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxNode(greenNode, tree, parent)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public bool IsVarArgs => FirstChild<LuaDotsToken>() != null;
}

public class LuaParamListSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxNode(greenNode, tree, parent)
{
    public IEnumerable<LuaParamDefSyntax> Params => ChildNodes<LuaParamDefSyntax>();

    public bool HasVarArgs => Params.LastOrDefault()?.IsVarArgs == true;
}

public class LuaAttributeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxNode(greenNode, tree, parent)
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

public class LuaLocalNameSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxNode(greenNode, tree, parent)
{
    public LuaAttributeSyntax? Attribute => FirstChild<LuaAttributeSyntax>();

    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaCallArgListSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxNode(greenNode, tree, parent)
{
    public IEnumerable<LuaExprSyntax> ArgList => ChildNodes<LuaExprSyntax>();

    public bool IsSingleArgCall => FirstChildToken(LuaTokenKind.TkLeftParen) != null;

    public LuaExprSyntax? SingleArg => FirstChild<LuaExprSyntax>();
}

public class LuaFuncBodySyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxNode(greenNode, tree, parent)
{
    public LuaParamListSyntax? ParamList => FirstChild<LuaParamListSyntax>();

    public LuaBlockSyntax? Block => FirstChild<LuaBlockSyntax>();
}

public interface IFuncBodyOwner
{
    public LuaFuncBodySyntax? FuncBody { get; }
}
