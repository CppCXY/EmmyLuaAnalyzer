using System.Collections;
using System.Collections.Immutable;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaDocSyntax : LuaSyntaxElement
{
    public IEnumerable<LuaSyntaxToken> Descriptions => ImmutableArray<LuaSyntaxToken>.Empty;
    // Tree.BinderData?.GetDescriptions(new LuaSyntaxNodeOrToken.Node(this)) ?? Enumerable.Empty<LuaCommentSyntax>();

    public LuaDocSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocBodySyntax : LuaDocSyntax
{
    public IEnumerable<LuaDocTypedFieldSyntax> FieldList => ChildNodes<LuaDocTypedFieldSyntax>();

    public LuaDocBodySyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree,
        parent)
    {
    }
}

public class LuaDocClassSyntax : LuaDocSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocGenericDeclareListSyntax? GenericDeclareList => FirstChild<LuaDocGenericDeclareListSyntax>();

    public bool HasExtendType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public IEnumerable<LuaDocTypeSyntax> ExtendTypeList => ChildNodes<LuaDocTypeSyntax>();

    public LuaDocBodySyntax? Body => FirstChild<LuaDocBodySyntax>();

    public LuaDocClassSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocGenericParamSyntax : LuaDocSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public LuaDocGenericParamSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocGenericDeclareListSyntax : LuaDocSyntax
{
    public IEnumerable<LuaDocGenericParamSyntax> Params => ChildNodes<LuaDocGenericParamSyntax>();

    public LuaDocGenericDeclareListSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocEnumSyntax : LuaDocSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public bool HasBaseType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public LuaDocTypeSyntax? BaseType => FirstChild<LuaDocTypeSyntax>();

    public IEnumerable<LuaDocEnumFieldSyntax> FieldList => ChildNodes<LuaDocEnumFieldSyntax>();

    public LuaDocEnumSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocInterfaceSyntax : LuaDocSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocGenericDeclareListSyntax? GenericDeclareList => FirstChild<LuaDocGenericDeclareListSyntax>();

    public bool HasExtendType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public IEnumerable<LuaDocTypeSyntax> ExtendTypeList => ChildNodes<LuaDocTypeSyntax>();

    public LuaDocBodySyntax? Body => FirstChild<LuaDocBodySyntax>();

    public LuaDocInterfaceSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocAliasSyntax : LuaDocSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public LuaDocAliasSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocFieldSyntax : LuaDocSyntax
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

    public LuaDocFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocParamSyntax : LuaDocSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public bool IsVarArgs => FirstChildToken(LuaTokenKind.TkDots) != null;

    public bool Nullable => FirstChildToken(LuaTokenKind.TkNullable) != null;

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public LuaDocParamSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocEnumFieldSyntax : LuaDocSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocEnumFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocReturnSyntax : LuaDocSyntax
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public IEnumerable<LuaDocTypeSyntax> TypeList => ChildNodes<LuaDocTypeSyntax>();

    public LuaDocReturnSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocSeeSyntax : LuaDocSyntax
{
    public LuaDocSeeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocOverloadSyntax : LuaDocSyntax
{
    public LuaDocFuncTypeSyntax? TypeFunc => FirstChild<LuaDocFuncTypeSyntax>();

    public LuaDocOverloadSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocDeprecatedSyntax : LuaDocSyntax
{
    public LuaDocDeprecatedSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTypedParamSyntax : LuaDocSyntax
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public LuaDocTypedParamSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocGenericSyntax : LuaDocSyntax
{
    public IEnumerable<LuaDocTypedParamSyntax> ParamList => ChildNodes<LuaDocTypedParamSyntax>();

    public LuaDocGenericSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocCastSyntax : LuaDocSyntax
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public LuaDocCastSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocAsyncSyntax : LuaDocSyntax
{
    public LuaDocAsyncSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocOtherSyntax : LuaDocSyntax
{
    public LuaDocOtherSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocVisibilitySyntax : LuaDocSyntax
{
    public VisibilityKind Visibility =>
        VisibilityKindHelper.ToVisibilityKind(FirstChildToken(LuaTokenKind.TkTagVisibility)!.Text);


    public LuaDocVisibilitySyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocNodiscardSyntax : LuaDocSyntax
{
    public LuaDocNodiscardSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocAsSyntax : LuaDocSyntax
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public LuaDocAsSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocVersionSyntax : LuaDocSyntax
{
    public IEnumerable<LuaNameToken> Versions => ChildNodes<LuaNameToken>();

    public LuaDocVersionSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocDiagnosticSyntax : LuaDocSyntax
{
    public LuaNameToken? State => FirstChild<LuaNameToken>();

    public IEnumerable<LuaNameToken> Diagnostics => ChildNodes<LuaNameToken>().Skip(1);

    public LuaDocDiagnosticSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocOperatorSyntax : LuaDocSyntax
{
    public LuaNameToken? Operator => FirstChild<LuaNameToken>();

    public IEnumerable<LuaDocTypeSyntax> Types => ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon);

    public LuaDocTypeSyntax? ReturnType => ChildNodeAfterToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon);

    public LuaDocOperatorSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocModuleSyntax : LuaDocSyntax
{
    public LuaStringToken? Module => FirstChild<LuaStringToken>();

    public LuaDocModuleSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}
