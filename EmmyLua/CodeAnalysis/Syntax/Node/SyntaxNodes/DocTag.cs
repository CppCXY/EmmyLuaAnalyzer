using System.Collections.Immutable;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaDocTagSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxNode(greenNode, tree, parent, startOffset)
{
    public LuaDescriptionSyntax? Description => FirstChild<LuaDescriptionSyntax>();
}

public interface ILuaDocTagNamedTypeSyntax
{
    public LuaNameToken? Name { get; }
}

public class LuaDocTagClassSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset), ILuaDocTagNamedTypeSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocGenericDeclareListSyntax? GenericDeclareList => FirstChild<LuaDocGenericDeclareListSyntax>();

    public bool HasExtendType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public IEnumerable<LuaDocTypeSyntax> ExtendTypeList => ChildNodes<LuaDocTypeSyntax>();

    public LuaDocBodySyntax? Body => FirstChild<LuaDocBodySyntax>();
}

public class LuaDocGenericParamSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxNode(greenNode, tree, parent, startOffset)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTagGenericSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaDocGenericParamSyntax> Params => ChildNodes<LuaDocGenericParamSyntax>();
}

public class LuaDocGenericDeclareListSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaDocGenericParamSyntax> Params => ChildNodes<LuaDocGenericParamSyntax>();
}

public class LuaDocTagEnumSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset), ILuaDocTagNamedTypeSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public bool HasBaseType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public LuaDocTypeSyntax? BaseType => FirstChild<LuaDocTypeSyntax>();

    public IEnumerable<LuaDocTagEnumFieldSyntax> FieldList => ChildNodes<LuaDocTagEnumFieldSyntax>();
}

public class LuaDocTagInterfaceSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset), ILuaDocTagNamedTypeSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocGenericDeclareListSyntax? GenericDeclareList => FirstChild<LuaDocGenericDeclareListSyntax>();

    public bool HasExtendType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public IEnumerable<LuaDocTypeSyntax> ExtendTypeList => ChildNodes<LuaDocTypeSyntax>();

    public LuaDocBodySyntax? Body => FirstChild<LuaDocBodySyntax>();
}

public class LuaDocTagAliasSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset), ILuaDocTagNamedTypeSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTagFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public LuaDocFieldSyntax? Field => FirstChild<LuaDocFieldSyntax>();
}

public class LuaDocTagParamSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaSyntaxToken? VarArgs => FirstChildToken(LuaTokenKind.TkDots);

    public bool Nullable => FirstChildToken(LuaTokenKind.TkNullable) != null;

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTagEnumFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaDocTagReturnSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public IEnumerable<LuaDocTypeSyntax> TypeList => ChildNodes<LuaDocTypeSyntax>();
}

public class LuaDocTagSeeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset);

public class LuaDocTagTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaDocTypeSyntax> TypeList => ChildNodes<LuaDocTypeSyntax>();
}

public class LuaDocTagOverloadSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public LuaDocFuncTypeSyntax? TypeFunc => FirstChild<LuaDocFuncTypeSyntax>();
}

public class LuaDocTagDeprecatedSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset);

public class LuaDocTagTypedParamSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDotsToken? VarArgs => FirstChild<LuaDotsToken>();

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public bool IsVarArgs => VarArgs != null;
}

public class LuaDocTagCastSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTagAsyncSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset);

public class LuaDocTagOtherSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset);

public class LuaDocTagVisibilitySyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public VisibilityKind Visibility =>
        VisibilityKindHelper.ToVisibilityKind(FirstChildToken(LuaTokenKind.TkTagVisibility)!.Text);
}

public class LuaDocTagNodiscardSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset);

public class LuaDocTagAsSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTagVersionSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaNameToken> Versions => ChildNodes<LuaNameToken>();
}

public class LuaDocTagDiagnosticSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public LuaNameToken? State => FirstChild<LuaNameToken>();

    public IEnumerable<LuaNameToken> Diagnostics => ChildNodes<LuaNameToken>().Skip(1);
}

public class LuaDocTagOperatorSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public LuaNameToken? Operator => FirstChild<LuaNameToken>();

    public IEnumerable<LuaDocTypeSyntax> Types => ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon);

    public LuaDocTypeSyntax? ReturnType => ChildNodeAfterToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon);
}

public class LuaDocTagModuleSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public LuaStringToken? Module => FirstChild<LuaStringToken>();
}
