using EmmyLuaAnalyzer.CodeAnalysis.Kind;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Green;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Tree;

namespace EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaCommentSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxNode(greenNode, tree, parent)
{
    public bool IsDeprecated => FirstChildToken(LuaTokenKind.TkTagDeprecated) != null;

    public IEnumerable<LuaDocTagSyntax> DocList => ChildNodes<LuaDocTagSyntax>();

    public IEnumerable<LuaSyntaxToken> Descriptions => ChildTokens(LuaTokenKind.TkDocDescription);

    public LuaSyntaxElement? Owner => Tree.BinderData?.CommentOwner(this);
}
