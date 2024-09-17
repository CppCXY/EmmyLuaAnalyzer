using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaCommentSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public bool IsDeprecated => FirstChild<LuaDocTagDeprecatedSyntax>() != null;

    public bool IsAsync => FirstChild<LuaDocTagAsyncSyntax>() != null;

    // TODO
    // public bool IsOverride => FirstChildToken(LuaTokenKind.TkTagOverride) != null;

    public IEnumerable<LuaDocTagSyntax> DocList => ChildrenElement<LuaDocTagSyntax>();

    public IEnumerable<LuaDescriptionSyntax> Descriptions => ChildrenElement<LuaDescriptionSyntax>();

    public string CommentText => string.Join("\n\n", Descriptions.Select(it => it.CommentText));

    public LuaSyntaxElement? Owner => Tree.BinderData?.CommentOwner(this);
}

public interface ICommentOwner
{
    IEnumerable<LuaCommentSyntax> Comments { get; }
}
