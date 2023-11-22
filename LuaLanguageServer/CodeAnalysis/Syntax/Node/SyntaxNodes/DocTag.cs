using System.Collections;
using System.Collections.Immutable;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaDocTagSyntax : LuaSyntaxElement
{
    public IEnumerable<LuaSyntaxToken> Descriptions => ImmutableArray<LuaSyntaxToken>.Empty;
    // Tree.BinderData?.GetDescriptions(new LuaSyntaxNodeOrToken.Node(this)) ?? Enumerable.Empty<LuaCommentSyntax>();

    public LuaDocTagSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagBodySyntax : LuaDocTagSyntax
{
    public IEnumerable<LuaDocTagTypedFieldSyntax> FieldList => ChildNodes<LuaDocTagTypedFieldSyntax>();

    public LuaDocTagBodySyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree,
        parent)
    {
    }
}

public class LuaDocTagClassSyntax : LuaDocTagSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTagGenericDeclareListSyntax? GenericDeclareList => FirstChild<LuaDocTagGenericDeclareListSyntax>();

    public bool HasExtendType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public IEnumerable<LuaDocTypeSyntax> ExtendTypeList => ChildNodes<LuaDocTypeSyntax>();

    public LuaDocTagBodySyntax? Body => FirstChild<LuaDocTagBodySyntax>();

    public LuaDocTagClassSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagGenericParamSyntax : LuaDocTagSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public LuaDocTagGenericParamSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagGenericDeclareListSyntax : LuaDocTagSyntax
{
    public IEnumerable<LuaDocTagGenericParamSyntax> Params => ChildNodes<LuaDocTagGenericParamSyntax>();

    public LuaDocTagGenericDeclareListSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagEnumSyntax : LuaDocTagSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public bool HasBaseType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public LuaDocTypeSyntax? BaseType => FirstChild<LuaDocTypeSyntax>();

    public IEnumerable<LuaDocTagEnumFieldSyntax> FieldList => ChildNodes<LuaDocTagEnumFieldSyntax>();

    public LuaDocTagEnumSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagInterfaceSyntax : LuaDocTagSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTagGenericDeclareListSyntax? GenericDeclareList => FirstChild<LuaDocTagGenericDeclareListSyntax>();

    public bool HasExtendType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public IEnumerable<LuaDocTypeSyntax> ExtendTypeList => ChildNodes<LuaDocTypeSyntax>();

    public LuaDocTagBodySyntax? Body => FirstChild<LuaDocTagBodySyntax>();

    public LuaDocTagInterfaceSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagAliasSyntax : LuaDocTagSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public LuaDocTagAliasSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagFieldSyntax : LuaDocTagSyntax
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

    public LuaDocTagFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagParamSyntax : LuaDocTagSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public bool IsVarArgs => FirstChildToken(LuaTokenKind.TkDots) != null;

    public bool Nullable => FirstChildToken(LuaTokenKind.TkNullable) != null;

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public LuaDocTagParamSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagEnumFieldSyntax : LuaDocTagSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTagEnumFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagReturnSyntax : LuaDocTagSyntax
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public IEnumerable<LuaDocTypeSyntax> TypeList => ChildNodes<LuaDocTypeSyntax>();

    public LuaDocTagReturnSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagSeeSyntax : LuaDocTagSyntax
{
    public LuaDocTagSeeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagTypeSyntax : LuaDocTagSyntax
{
    public IEnumerable<LuaDocTypeSyntax> TypeList => ChildNodes<LuaDocTypeSyntax>();

    public LuaDocTagTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagOverloadSyntax : LuaDocTagSyntax
{
    public LuaDocFuncTypeSyntax? TypeFunc => FirstChild<LuaDocFuncTypeSyntax>();

    public LuaDocTagOverloadSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagDeprecatedSyntax : LuaDocTagSyntax
{
    public LuaDocTagDeprecatedSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagTypedParamSyntax : LuaDocTagSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public LuaDocTagTypedParamSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagGenericSyntax : LuaDocTagSyntax
{
    public IEnumerable<LuaDocTagTypedParamSyntax> ParamList => ChildNodes<LuaDocTagTypedParamSyntax>();

    public LuaDocTagGenericSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagCastSyntax : LuaDocTagSyntax
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public LuaDocTagCastSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagAsyncSyntax : LuaDocTagSyntax
{
    public LuaDocTagAsyncSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagOtherSyntax : LuaDocTagSyntax
{
    public LuaDocTagOtherSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagVisibilitySyntax : LuaDocTagSyntax
{
    public VisibilityKind Visibility =>
        VisibilityKindHelper.ToVisibilityKind(FirstChildToken(LuaTokenKind.TkTagVisibility)!.Text);


    public LuaDocTagVisibilitySyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagNodiscardSyntax : LuaDocTagSyntax
{
    public LuaDocTagNodiscardSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagAsSyntax : LuaDocTagSyntax
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public LuaDocTagAsSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagVersionSyntax : LuaDocTagSyntax
{
    public IEnumerable<LuaNameToken> Versions => ChildNodes<LuaNameToken>();

    public LuaDocTagVersionSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagDiagnosticSyntax : LuaDocTagSyntax
{
    public LuaNameToken? State => FirstChild<LuaNameToken>();

    public IEnumerable<LuaNameToken> Diagnostics => ChildNodes<LuaNameToken>().Skip(1);

    public LuaDocTagDiagnosticSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagOperatorSyntax : LuaDocTagSyntax
{
    public LuaNameToken? Operator => FirstChild<LuaNameToken>();

    public IEnumerable<LuaDocTypeSyntax> Types => ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon);

    public LuaDocTypeSyntax? ReturnType => ChildNodeAfterToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon);

    public LuaDocTagOperatorSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTagModuleSyntax : LuaDocTagSyntax
{
    public LuaStringToken? Module => FirstChild<LuaStringToken>();

    public LuaDocTagModuleSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}
