using System.Collections;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaDocTypeSyntax : LuaSyntaxElement
{
    public LuaSyntaxToken? Description => FirstChildToken(LuaTokenKind.TkDocDescription);

    public LuaDocTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocLiteralTypeSyntax : LuaDocTypeSyntax
{
    public bool IsInteger => FirstChild<LuaIntegerToken>() != null;

    public bool IsString => FirstChild<LuaStringToken>() != null;

    public LuaIntegerToken? Integer => FirstChild<LuaIntegerToken>();

    public LuaStringToken? String => FirstChild<LuaStringToken>();

    public LuaDocLiteralTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocNameTypeSyntax : LuaDocTypeSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocNameTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocArrayTypeSyntax : LuaDocTypeSyntax
{
    public LuaDocTypeSyntax? BaseType => FirstChild<LuaDocTypeSyntax>();

    public LuaDocArrayTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTableTypeSyntax : LuaDocTypeSyntax
{
    public IEnumerable<LuaDocTypedFieldSyntax> FieldList => ChildNodes<LuaDocTypedFieldSyntax>();

    public LuaDocTableTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocFuncTypeSyntax : LuaDocTypeSyntax
{
    public IEnumerable<LuaDocTypedParamSyntax> ParamList => ChildNodes<LuaDocTypedParamSyntax>();

    public LuaDocTypeSyntax? ReturnType => FirstChild<LuaDocTypeSyntax>();

    public LuaDocFuncTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocUnionTypeSyntax : LuaDocTypeSyntax
{
    public IEnumerable<LuaDocTypeSyntax> UnionTypes => ChildNodes<LuaDocTypeSyntax>();

    public LuaDocUnionTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTupleTypeSyntax : LuaDocTypeSyntax
{
    public IEnumerable<LuaDocTypeSyntax> TypeList => ChildNodes<LuaDocTypeSyntax>();

    public LuaDocTupleTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocParenTypeSyntax : LuaDocTypeSyntax
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public LuaDocParenTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTypedFieldSyntax : LuaDocSyntax
{
    public bool IsNameKey => FirstChild<LuaNameToken>() != null;

    public bool IsStringKey => FirstChild<LuaStringToken>() != null;

    public bool IsIntegerKey => FirstChild<LuaIntegerToken>() != null;

    public bool IsTypeKey => ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon).FirstOrDefault() != null;

    public LuaNameToken? NameKey => FirstChild<LuaNameToken>();

    public LuaStringToken? StringKey => FirstChild<LuaStringToken>();

    public LuaIntegerToken? IntegerKey => FirstChild<LuaIntegerToken>();

    public LuaDocTypeSyntax? TypeKey => ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon).FirstOrDefault();

    public LuaDocTypeSyntax? Type => ChildNodesAfterToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon).FirstOrDefault();

    public LuaDocTypedFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}
