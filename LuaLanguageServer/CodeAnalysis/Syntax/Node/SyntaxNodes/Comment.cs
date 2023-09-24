using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaCommentSyntax : LuaSyntaxNode
{
    public bool IsDeprecated => FirstChildToken(LuaTokenKind.TkTagDeprecated) != null;

    public IEnumerable<LuaDocSyntax> DocList => ChildNodes<LuaDocSyntax>();

    public IEnumerable<LuaSyntaxToken> Descriptions => ChildTokens(LuaTokenKind.TkDocDescription);

    public LuaCommentSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }

    public LuaSyntaxElement? Owner => Tree.BinderData?.CommentOwner(this);
}
