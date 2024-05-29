using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Syntax.Tree.Green;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaCommentSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
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
