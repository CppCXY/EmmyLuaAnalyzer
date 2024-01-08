using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaCommentSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxNode(greenNode, tree, parent)
{
    public bool IsDeprecated => FirstChildToken(LuaTokenKind.TkTagDeprecated) != null;

    public IEnumerable<LuaDocTagSyntax> DocList => ChildNodes<LuaDocTagSyntax>();

    public IEnumerable<LuaSyntaxToken> Descriptions => ChildTokens(LuaTokenKind.TkDocDescription);

    public LuaSyntaxElement? Owner => Tree.BinderData?.CommentOwner(this);
}
