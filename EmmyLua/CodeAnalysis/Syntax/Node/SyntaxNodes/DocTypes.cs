using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaDocTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxElement(greenNode, tree, parent, startOffset)
{
    public LuaSyntaxToken? Description => FirstChildToken(LuaTokenKind.TkDocDescription);
}

public class LuaDocLiteralTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTypeSyntax(greenNode, tree, parent, startOffset)
{
    public bool IsInteger => FirstChild<LuaIntegerToken>() != null;

    public bool IsString => FirstChild<LuaStringToken>() != null;

    public LuaIntegerToken? Integer => FirstChild<LuaIntegerToken>();

    public LuaStringToken? String => FirstChild<LuaStringToken>();
}

public class LuaDocNameTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTypeSyntax(greenNode, tree, parent, startOffset)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaDocArrayTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTypeSyntax(greenNode, tree, parent, startOffset)
{
    public LuaDocTypeSyntax? BaseType => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTableTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTypeSyntax(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaDocTagTypedFieldSyntax> FieldList => ChildNodes<LuaDocTagTypedFieldSyntax>();
}

public class LuaDocFuncTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTypeSyntax(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaDocTagTypedParamSyntax> ParamList => ChildNodes<LuaDocTagTypedParamSyntax>();

    public IEnumerable<LuaDocTypeSyntax> ReturnType => ChildNodes<LuaDocTypeSyntax>();
}

public class LuaDocUnionTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTypeSyntax(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaDocTypeSyntax> UnionTypes => ChildNodes<LuaDocTypeSyntax>();
}

public class LuaDocTupleTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTypeSyntax(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaDocTypeSyntax> TypeList => ChildNodes<LuaDocTypeSyntax>();
}

public class LuaDocParenTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTypeSyntax(greenNode, tree, parent, startOffset)
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTagTypedFieldSyntax(
    GreenNode greenNode,
    LuaSyntaxTree tree,
    LuaSyntaxElement? parent,
    int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public bool IsNameField => FirstChild<LuaNameToken>() != null;

    public bool IsStringField => FirstChild<LuaStringToken>() != null;

    public bool IsIntegerField => FirstChild<LuaIntegerToken>() != null;

    public bool IsTypeField => ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon).FirstOrDefault() != null;

    public LuaNameToken? NameField => FirstChild<LuaNameToken>();

    public LuaStringToken? StringField => FirstChild<LuaStringToken>();

    public LuaIntegerToken? IntegerField => FirstChild<LuaIntegerToken>();

    public LuaDocTypeSyntax? TypeField =>
        ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon).FirstOrDefault();

    public LuaDocTypeSyntax? Type => ChildNodesAfterToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon).FirstOrDefault();
}

public class LuaDocGenericTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTypeSyntax(greenNode, tree, parent, startOffset)
{
    public LuaDocNameTypeSyntax? Name => FirstChild<LuaDocNameTypeSyntax>();
    public IEnumerable<LuaDocTypeSyntax> GenericArgs => ChildNodes<LuaDocTypeSyntax>();
}
