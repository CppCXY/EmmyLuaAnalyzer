using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaCommentSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxNode(greenNode, tree, parent, startOffset)
{
    public bool IsDeprecated => FirstChild<LuaDocTagDeprecatedSyntax>() != null;

    public bool IsAsync => FirstChild<LuaDocTagAsyncSyntax>() != null;

    // TODO
    // public bool IsOverride => FirstChildToken(LuaTokenKind.TkTagOverride) != null;

    public IEnumerable<LuaDocTagSyntax> DocList => ChildNodes<LuaDocTagSyntax>();

    public IEnumerable<LuaDescriptionSyntax> Descriptions => ChildNodes<LuaDescriptionSyntax>();

    public string CommentText => string.Join("\n\n", Descriptions.Select(it => it.CommentText));

    public LuaSyntaxElement? Owner => Tree.BinderData?.CommentOwner(this);
}
