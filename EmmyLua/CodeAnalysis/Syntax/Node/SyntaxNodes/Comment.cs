using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaCommentSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public static bool CanOwner(LuaSyntaxKind kind) => kind switch
    {
        _ when LuaStatSyntax.CanCast(kind) => true,
        LuaSyntaxKind.TableFieldAssign or LuaSyntaxKind.TableFieldValue => true,
        _ => false
    };

    public IEnumerable<LuaDocTagSyntax> DocList => Iter.ChildrenNodeOfType<LuaDocTagSyntax>(LuaDocTagSyntax.CanCast);

    public IEnumerable<LuaDescriptionSyntax> Descriptions =>
        Iter.ChildrenNodeOfType<LuaDescriptionSyntax>(LuaSyntaxKind.Description);

    public string CommentText => string.Join("\n\n", Descriptions.Select(it => it.CommentText));

    public LuaSyntaxElement? Owner => Tree.BinderData?.CommentOwner(this);
}

public interface ICommentOwner
{
    IEnumerable<LuaCommentSyntax> Comments { get; }
}
