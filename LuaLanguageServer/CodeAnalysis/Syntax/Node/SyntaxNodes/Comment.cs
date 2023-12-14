using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaCommentSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxNode(greenNode, tree, parent)
{
    public bool IsDeprecated => FirstChildToken(LuaTokenKind.TkTagDeprecated) != null;

    public IEnumerable<LuaDocTagSyntax> DocList => ChildNodes<LuaDocTagSyntax>();

    public IEnumerable<LuaSyntaxToken> Descriptions => ChildTokens(LuaTokenKind.TkDocDescription);

    public LuaSyntaxElement? Owner => Tree.BinderData?.CommentOwner(this);
}
