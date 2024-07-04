using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaDocTagSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public LuaDescriptionSyntax? Description => FirstChild<LuaDescriptionSyntax>();
}

public abstract class LuaDocTagNamedTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaDocTagClassSyntax(int index, LuaSyntaxTree tree) : LuaDocTagNamedTypeSyntax(index, tree)
{
    public LuaDocGenericDeclareListSyntax? GenericDeclareList => FirstChild<LuaDocGenericDeclareListSyntax>();

    public bool HasExtendType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public IEnumerable<LuaDocTypeSyntax> ExtendTypeList => ChildrenElement<LuaDocTypeSyntax>();

    public LuaDocBodySyntax? Body => FirstChild<LuaDocBodySyntax>();
}

public class LuaDocGenericParamSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTagGenericSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public IEnumerable<LuaDocGenericParamSyntax> Params => ChildrenElement<LuaDocGenericParamSyntax>();
}

public class LuaDocGenericDeclareListSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public IEnumerable<LuaDocGenericParamSyntax> Params => ChildrenElement<LuaDocGenericParamSyntax>();
}

public class LuaDocTagEnumSyntax(int index, LuaSyntaxTree tree) : LuaDocTagNamedTypeSyntax(index, tree)
{
    public bool HasBaseType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public LuaDocTypeSyntax? BaseType => FirstChild<LuaDocTypeSyntax>();

    public IEnumerable<LuaDocTagEnumFieldSyntax> FieldList => ChildrenElement<LuaDocTagEnumFieldSyntax>();
}

public class LuaDocTagInterfaceSyntax(int index, LuaSyntaxTree tree) : LuaDocTagNamedTypeSyntax(index, tree)
{
    public LuaDocGenericDeclareListSyntax? GenericDeclareList => FirstChild<LuaDocGenericDeclareListSyntax>();

    public bool HasExtendType => FirstChildToken(LuaTokenKind.TkColon) != null;

    public IEnumerable<LuaDocTypeSyntax> ExtendTypeList => ChildrenElement<LuaDocTypeSyntax>();

    public LuaDocBodySyntax? Body => FirstChild<LuaDocBodySyntax>();
}

public class LuaDocTagAliasSyntax(int index, LuaSyntaxTree tree) : LuaDocTagNamedTypeSyntax(index, tree)
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTagFieldSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaDocFieldSyntax? Field => FirstChild<LuaDocFieldSyntax>();
}

public class LuaDocTagParamSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaSyntaxToken? VarArgs => FirstChildToken(LuaTokenKind.TkDots);

    public bool Nullable => FirstChildToken(LuaTokenKind.TkNullable) != null;

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTagEnumFieldSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaDocTagReturnSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public IEnumerable<LuaDocTypeSyntax> TypeList => ChildrenElement<LuaDocTypeSyntax>();
}

public class LuaDocTagSeeSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree);

public class LuaDocTagTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public IEnumerable<LuaDocTypeSyntax> TypeList => ChildrenElement<LuaDocTypeSyntax>();
}

public class LuaDocTagOverloadSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaDocFuncTypeSyntax? TypeFunc => FirstChild<LuaDocFuncTypeSyntax>();
}

public class LuaDocTagDeprecatedSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree);

public class LuaDocTagCastSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTagAsyncSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree);

public class LuaDocTagOtherSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree);

public class LuaDocTagVisibilitySyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public VisibilityKind Visibility =>
        VisibilityKindHelper.ToVisibilityKind(FirstChildToken(LuaTokenKind.TkTagVisibility)!.Text);
}

public class LuaDocTagNodiscardSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree);

public class LuaDocTagAsSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTagVersionSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public IEnumerable<LuaDocVersionSyntax> Versions => ChildrenElement<LuaDocVersionSyntax>();
}

public class LuaDocTagDiagnosticSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaNameToken? Action => FirstChild<LuaNameToken>();

    public LuaDocDiagnosticNameListSyntax? Diagnostics => FirstChild<LuaDocDiagnosticNameListSyntax>();
}

public class LuaDocDiagnosticNameListSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public IEnumerable<LuaNameToken> DiagnosticNames => ChildrenElement<LuaNameToken>();
}

public class LuaDocTagOperatorSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaNameToken? Operator => FirstChild<LuaNameToken>();

    public IEnumerable<LuaDocTypeSyntax> Types => ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon);

    public LuaDocTypeSyntax? ReturnType => ChildNodeAfterToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon);
}

public class LuaDocTagMetaSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree);

public class LuaDocTagModuleSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaStringToken? Module => FirstChild<LuaStringToken>();

    public LuaNameToken? Action => FirstChild<LuaNameToken>();
}

public class LuaDocTagMappingSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaDocAttributeSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public IEnumerable<LuaNameToken> Attributes => ChildrenElement<LuaNameToken>();
}
