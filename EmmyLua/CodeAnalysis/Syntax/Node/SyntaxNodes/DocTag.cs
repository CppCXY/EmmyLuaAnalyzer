using System.Collections;
using System.Collections.Immutable;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaDocTagSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxElement(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaSyntaxToken> Descriptions => ImmutableArray<LuaSyntaxToken>.Empty;
    // Tree.BinderData?.GetDescriptions(new LuaSyntaxNodeOrToken.Node(this)) ?? Enumerable.Empty<LuaCommentSyntax>();
}

public class LuaDocTagBodySyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaDocTagTypedFieldSyntax> FieldList => ChildNodes<LuaDocTagTypedFieldSyntax>();
}

public class LuaDocTagClassSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTagGenericDeclareListSyntax? GenericDeclareList => FirstChild<LuaDocTagGenericDeclareListSyntax>();

    public bool HasExtendType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public IEnumerable<LuaDocTypeSyntax> ExtendTypeList => ChildNodes<LuaDocTypeSyntax>();

    public LuaDocTagBodySyntax? Body => FirstChild<LuaDocTagBodySyntax>();
}

public class LuaDocGenericParamSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxElement(greenNode, tree, parent, startOffset)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTagGenericDeclareListSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public IEnumerable<LuaDocGenericParamSyntax> Params => ChildNodes<LuaDocGenericParamSyntax>();
}

public class LuaDocTagEnumSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public bool HasBaseType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public LuaDocTypeSyntax? BaseType => FirstChild<LuaDocTypeSyntax>();

    public IEnumerable<LuaDocTagEnumFieldSyntax> FieldList => ChildNodes<LuaDocTagEnumFieldSyntax>();
}

public class LuaDocTagInterfaceSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTagGenericDeclareListSyntax? GenericDeclareList => FirstChild<LuaDocTagGenericDeclareListSyntax>();

    public bool HasExtendType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public IEnumerable<LuaDocTypeSyntax> ExtendTypeList => ChildNodes<LuaDocTypeSyntax>();

    public LuaDocTagBodySyntax? Body => FirstChild<LuaDocTagBodySyntax>();
}

public class LuaDocTagAliasSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTagFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaDocTagSyntax(greenNode, tree, parent, startOffset)
{
    public VisibilityKind Visibility
    {
        get
        {
            var tk = FirstChildToken(LuaTokenKind.TkTagVisibility);
            if (tk == null)
            {
                return VisibilityKind.Public;
            }

            return VisibilityKindHelper.ToVisibilityKind(tk.Text);
        }
    }

    public bool IsNameField => FirstChildToken(LuaTokenKind.TkName) != null;

    public bool IsStringField => FirstChildToken(LuaTokenKind.TkString) != null;

    public bool IsIntegerField => FirstChildToken(LuaTokenKind.TkInt) != null;

    public bool IsTypeField =>
        ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkRightBracket).FirstOrDefault() != null;

    public LuaNameToken? NameField => FirstChild<LuaNameToken>();

    public LuaStringToken? StringField => FirstChild<LuaStringToken>();

    public LuaIntegerToken? IntegerField => FirstChild<LuaIntegerToken>();

    public LuaDocTypeSyntax? TypeField =>
        ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkRightBracket).FirstOrDefault();

    public bool Nullable => FirstChildToken(LuaTokenKind.TkNullable) != null;

    public LuaDocTypeSyntax? Type => IsTypeField
        ? ChildNodes<LuaDocTypeSyntax>().Skip(1).FirstOrDefault()
        : FirstChild<LuaDocTypeSyntax>();
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
