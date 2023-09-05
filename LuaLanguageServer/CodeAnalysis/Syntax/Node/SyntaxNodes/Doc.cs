using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaDocSyntax : LuaSyntaxNode
{
    public LuaSyntaxToken? Description => FirstChildToken(LuaTokenKind.TkDocDescription);

    public LuaDocSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocClassSyntax : LuaDocSyntax
{
    public LuaSyntaxToken? Name => FirstChildToken(LuaTokenKind.TkName);

    public LuaDocGenericDeclareList? GenericDeclareList => FirstChild<LuaDocGenericDeclareList>();

    public bool HasExtendType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public IEnumerable<LuaDocType> ExtendTypeList => ChildNodes<LuaDocType>();

    public LuaDocClassSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocGenericDeclareList : LuaDocSyntax
{
    public IEnumerable<LuaSyntaxToken> NameList => ChildTokens(LuaTokenKind.TkName);

    public LuaDocGenericDeclareList(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocEnumSyntax : LuaDocSyntax
{
    public LuaSyntaxToken? Name => FirstChildToken(LuaTokenKind.TkName);

    public bool HasBaseType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public LuaDocType? BaseType => FirstChild<LuaDocType>();

    public LuaDocEnumSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocInterfaceSyntax : LuaDocSyntax
{
    public LuaSyntaxToken? Name => FirstChildToken(LuaTokenKind.TkName);

    public LuaDocGenericDeclareList? GenericDeclareList => FirstChild<LuaDocGenericDeclareList>();

    public bool HasExtendType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public IEnumerable<LuaDocType> ExtendTypeList => ChildNodes<LuaDocType>();

    public LuaDocInterfaceSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocAliasSyntax : LuaDocSyntax
{
    public LuaSyntaxToken? Name => FirstChildToken(LuaTokenKind.TkName);

    public LuaDocType? Type => FirstChild<LuaDocType>();

    public LuaDocAliasSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTypeSyntax : LuaDocSyntax
{
    public LuaDocType? Type => FirstChild<LuaDocType>();

    public LuaDocTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}

// TODO
public class LuaDocFieldSyntax : LuaDocSyntax
{
    public LuaSyntaxToken? Name => FirstChildToken(LuaTokenKind.TkName);

    public bool Nullable => FirstChildToken(LuaTokenKind.TkNullable) != null;

    public LuaDocType? Type => FirstChild<LuaDocType>();

    public LuaDocFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocParamSyntax : LuaDocSyntax
{
    public LuaSyntaxToken? Name => FirstChildToken(LuaTokenKind.TkName);

    public bool IsVarArgs => FirstChildToken(LuaTokenKind.TkDots) != null;

    public bool Nullable => FirstChildToken(LuaTokenKind.TkNullable) != null;

    public LuaDocType? Type => FirstChild<LuaDocType>();

    public LuaDocParamSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocEnumFieldSyntax : LuaDocSyntax
{
    public LuaDocLiteralType? Literal => FirstChild<LuaDocLiteralType>();

    public LuaDocEnumFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocReturnSyntax : LuaDocSyntax
{
    public LuaDocType? Type => FirstChild<LuaDocType>();

    public IEnumerable<LuaDocType> TypeList => ChildNodes<LuaDocType>();

    public LuaDocReturnSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}
