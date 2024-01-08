using System.Collections;
using EmmyLuaAnalyzer.CodeAnalysis.Kind;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Green;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Tree;

namespace EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaDocTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxElement(greenNode, tree, parent)
{
    public LuaSyntaxToken? Description => FirstChildToken(LuaTokenKind.TkDocDescription);
}

public class LuaDocLiteralTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaDocTypeSyntax(greenNode, tree, parent)
{
    public bool IsInteger => FirstChild<LuaIntegerToken>() != null;

    public bool IsString => FirstChild<LuaStringToken>() != null;

    public LuaIntegerToken? Integer => FirstChild<LuaIntegerToken>();

    public LuaStringToken? String => FirstChild<LuaStringToken>();
}

public class LuaDocNameTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaDocTypeSyntax(greenNode, tree, parent)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaDocArrayTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaDocTypeSyntax(greenNode, tree, parent)
{
    public LuaDocTypeSyntax? BaseType => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTableTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaDocTypeSyntax(greenNode, tree, parent)
{
    public IEnumerable<LuaDocTagTypedFieldSyntax> FieldList => ChildNodes<LuaDocTagTypedFieldSyntax>();
}

public class LuaDocFuncTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaDocTypeSyntax(greenNode, tree, parent)
{
    public IEnumerable<LuaDocTagTypedParamSyntax> ParamList => ChildNodes<LuaDocTagTypedParamSyntax>();

    public LuaDocTypeSyntax? ReturnType => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocUnionTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaDocTypeSyntax(greenNode, tree, parent)
{
    public IEnumerable<LuaDocTypeSyntax> UnionTypes => ChildNodes<LuaDocTypeSyntax>();
}

public class LuaDocTupleTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaDocTypeSyntax(greenNode, tree, parent)
{
    public IEnumerable<LuaDocTypeSyntax> TypeList => ChildNodes<LuaDocTypeSyntax>();
}

public class LuaDocParenTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaDocTypeSyntax(greenNode, tree, parent)
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTagTypedFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaDocTagSyntax(greenNode, tree, parent)
{
    public bool IsNameField => FirstChild<LuaNameToken>() != null;

    public bool IsStringField => FirstChild<LuaStringToken>() != null;

    public bool IsIntegerField => FirstChild<LuaIntegerToken>() != null;

    public bool IsTypeField => ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon).FirstOrDefault() != null;

    public LuaNameToken? NameField => FirstChild<LuaNameToken>();

    public LuaStringToken? StringField => FirstChild<LuaStringToken>();

    public LuaIntegerToken? IntegerField => FirstChild<LuaIntegerToken>();

    public LuaDocTypeSyntax? TypeField => ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon).FirstOrDefault();

    public LuaDocTypeSyntax? Type => ChildNodesAfterToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon).FirstOrDefault();
}
