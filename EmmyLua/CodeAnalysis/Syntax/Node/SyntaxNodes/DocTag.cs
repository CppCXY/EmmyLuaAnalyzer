using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaDocTagSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public static bool CanCast(LuaSyntaxKind kind) =>
        kind is >= LuaSyntaxKind.DocClass and <= LuaSyntaxKind.DocReadonly;

    public LuaDescriptionSyntax? Description =>
        Iter.FirstChildNode(LuaSyntaxKind.Description).ToNode<LuaDescriptionSyntax>();
}

public abstract class LuaDocTagNamedTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaNameToken? Name => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();

    public LuaDocAttributeSyntax? Attribute =>
        Iter.FirstChildNode(LuaSyntaxKind.DocAttribute).ToNode<LuaDocAttributeSyntax>();
}

public class LuaDocTagClassSyntax : LuaDocTagNamedTypeSyntax
{
    public LuaDocTagClassSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.Kind == LuaSyntaxKind.GenericDeclareList)
            {
                _genericDeclareListIndex = it.Index;
            }
            else if (LuaDocTypeSyntax.CanCast(it.Kind))
            {
                _extendTypeListIndex.Add(it.Index);
            }
            else if (it.Kind == LuaSyntaxKind.DocBody)
            {
                _bodyIndex = it.Index;
            }
        }
    }

    private int _genericDeclareListIndex = -1;

    public LuaDocGenericDeclareListSyntax? GenericDeclareList =>
        Tree.GetElement<LuaDocGenericDeclareListSyntax>(_genericDeclareListIndex);

    public bool HasExtendType => _extendTypeListIndex.Count > 0;

    private List<int> _extendTypeListIndex = [];

    public IEnumerable<LuaDocTypeSyntax> ExtendTypeList => Tree.GetElements<LuaDocTypeSyntax>(_extendTypeListIndex);

    private int _bodyIndex = -1;

    public LuaDocBodySyntax? Body => Tree.GetElement<LuaDocBodySyntax>(_bodyIndex);
}

public class LuaDocGenericParamSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public LuaNameToken? Name => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();

    public LuaDocTypeSyntax? Type => Iter.FirstChildNode(LuaSyntaxKind.DocType).ToNode<LuaDocTypeSyntax>();
}

public class LuaDocTagGenericSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public IEnumerable<LuaDocGenericParamSyntax> Params =>
        Iter.ChildrenNodeOfType<LuaDocGenericParamSyntax>(LuaSyntaxKind.GenericParameter);
}

public class LuaDocGenericDeclareListSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public IEnumerable<LuaDocGenericParamSyntax> Params =>
        Iter.ChildrenNodeOfType<LuaDocGenericParamSyntax>(LuaSyntaxKind.GenericParameter);
}

public class LuaDocTagEnumSyntax : LuaDocTagNamedTypeSyntax
{
    public LuaDocTagEnumSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (LuaDocTypeSyntax.CanCast(it.Kind))
            {
                _baseTypeIndex = it.Index;
            }
            else if (it.Kind == LuaSyntaxKind.DocEnumField)
            {
                _fieldListIndex.Add(it.Index);
            }
        }
    }

    public bool HasBaseType => _baseTypeIndex != -1;

    private int _baseTypeIndex = -1;

    public LuaDocTypeSyntax? BaseType => Tree.GetElement<LuaDocTypeSyntax>(_baseTypeIndex);

    private List<int> _fieldListIndex = [];

    public IEnumerable<LuaDocTagEnumFieldSyntax> FieldList =>
        Tree.GetElements<LuaDocTagEnumFieldSyntax>(_fieldListIndex);
}

public class LuaDocTagInterfaceSyntax : LuaDocTagNamedTypeSyntax
{
    public LuaDocTagInterfaceSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.Kind == LuaSyntaxKind.GenericDeclareList)
            {
                _genericDeclareListIndex = it.Index;
            }
            else if (LuaDocTypeSyntax.CanCast(it.Kind))
            {
                _extendTypeListIndex.Add(it.Index);
            }
            else if (it.Kind == LuaSyntaxKind.DocBody)
            {
                _bodyIndex = it.Index;
            }
        }
    }

    private int _genericDeclareListIndex = -1;

    public LuaDocGenericDeclareListSyntax? GenericDeclareList =>
        Tree.GetElement<LuaDocGenericDeclareListSyntax>(_genericDeclareListIndex);

    public bool HasExtendType => _extendTypeListIndex.Count > 0;

    private List<int> _extendTypeListIndex = [];

    public IEnumerable<LuaDocTypeSyntax> ExtendTypeList => Tree.GetElements<LuaDocTypeSyntax>(_extendTypeListIndex);

    private int _bodyIndex = -1;

    public LuaDocBodySyntax? Body => Tree.GetElement<LuaDocBodySyntax>(_bodyIndex);
}

public class LuaDocTagAliasSyntax(int index, LuaSyntaxTree tree) : LuaDocTagNamedTypeSyntax(index, tree)
{
    public LuaDocGenericDeclareListSyntax? GenericDeclareList => Iter.FirstChildNode(LuaSyntaxKind.GenericDeclareList)
        .ToNode<LuaDocGenericDeclareListSyntax>();

    public LuaDocTypeSyntax? Type => Iter.FirstChildNode(LuaSyntaxKind.DocType).ToNode<LuaDocTypeSyntax>();
}

public class LuaDocTagFieldSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaDocFieldSyntax? Field => Iter.FirstChildNode(LuaSyntaxKind.DocDetailField).ToNode<LuaDocFieldSyntax>();
}

public class LuaDocTagParamSyntax : LuaDocTagSyntax
{
    public LuaDocTagParamSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkDots)
            {
                _varArgsIndex = it.Index;
            }
            else if (it.TokenKind == LuaTokenKind.TkDocQuestion)
            {
                Nullable = true;
            }
            else if (it.TokenKind == LuaTokenKind.TkName)
            {
                _nameIndex = it.Index;
            }
            else if (LuaDocTypeSyntax.CanCast(it.Kind))
            {
                _typeIndex = it.Index;
            }
        }
    }

    private int _nameIndex = -1;

    public LuaNameToken? Name => Tree.GetElement<LuaNameToken>(_nameIndex);

    private int _varArgsIndex = -1;

    public LuaSyntaxToken? VarArgs => Tree.GetElement<LuaSyntaxToken>(_varArgsIndex);

    public bool Nullable { get; }

    private int _typeIndex = -1;

    public LuaDocTypeSyntax? Type => Tree.GetElement<LuaDocTypeSyntax>(_typeIndex);
}

public class LuaDocTagEnumFieldSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaNameToken? Name => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();
}

public class LuaDocTagReturnSyntax : LuaDocTagSyntax
{
    public LuaDocTagReturnSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (LuaDocTypeSyntax.CanCast(it.Kind))
            {
                if (_typeListIndex.Count == 0)
                {
                    _typeIndex = it.Index;
                    _typeListIndex.Add(it.Index);
                }
                else
                {
                    _typeListIndex.Add(it.Index);
                }
            }
        }
    }

    private int _typeIndex = -1;

    public LuaDocTypeSyntax? Type => Tree.GetElement<LuaDocTypeSyntax>(_typeIndex);

    private List<int> _typeListIndex = [];

    public IEnumerable<LuaDocTypeSyntax> TypeList => Tree.GetElements<LuaDocTypeSyntax>(_typeListIndex);
}

public class LuaDocTagSeeSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree);

public class LuaDocTagTypeSyntax : LuaDocTagSyntax
{
    public LuaDocTagTypeSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (LuaDocTypeSyntax.CanCast(it.Kind))
            {
                _typeListIndex.Add(it.Index);
            }
        }
    }

    private List<int> _typeListIndex = [];

    public IEnumerable<LuaDocTypeSyntax> TypeList => Tree.GetElements<LuaDocTypeSyntax>(_typeListIndex);
}

public class LuaDocTagOverloadSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaDocFuncTypeSyntax? TypeFunc => Iter.FirstChildNode(LuaSyntaxKind.TypeFun).ToNode<LuaDocFuncTypeSyntax>();
}

public class LuaDocTagDeprecatedSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree);

public class LuaDocTagCastSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaDocTypeSyntax? Type => Iter.FirstChildNode(LuaDocTypeSyntax.CanCast).ToNode<LuaDocTypeSyntax>();
}

public class LuaDocTagAsyncSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree);

public class LuaDocTagOtherSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree);

public class LuaDocTagVisibilitySyntax : LuaDocTagSyntax
{
    public LuaDocTagVisibilitySyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkTagVisibility)
            {
                _visibilityIndex = it.Index;
            }
        }
    }

    private int _visibilityIndex = -1;

    public VisibilityKind Visibility =>
        VisibilityKindHelper.ToVisibilityKind(Tree.GetElement<LuaSyntaxToken>(_visibilityIndex)!.Text);
}

public class LuaDocTagNodiscardSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree);

public class LuaDocTagAsSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    // TODO impl this
    // public LuaDocTypeSyntax? Type =>
}

public class LuaDocTagVersionSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public IEnumerable<LuaDocVersionSyntax> Versions =>
        Iter.ChildrenNodeOfType<LuaDocVersionSyntax>(LuaSyntaxKind.DocVersion);
}

public class LuaDocTagDiagnosticSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaNameToken? Action => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();

    public LuaDocDiagnosticNameListSyntax? Diagnostics => Iter.FirstChildNode(LuaSyntaxKind.DiagnosticNameList)
        .ToNode<LuaDocDiagnosticNameListSyntax>();
}

public class LuaDocDiagnosticNameListSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public IEnumerable<LuaNameToken> DiagnosticNames => Iter.ChildrenTokenOfType<LuaNameToken>(LuaTokenKind.TkName);
}

public class LuaDocTagOperatorSyntax : LuaDocTagSyntax
{
    public LuaDocTagOperatorSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        var foundColon = false;
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkName)
            {
                _operatorIndex = it.Index;
            }
            else if (LuaDocTypeSyntax.CanCast(it.Kind))
            {
                if (!foundColon)
                {
                    _paramTypesIndex.Add(it.Index);
                }
                else
                {
                    _returnTypeIndex = it.Index;
                }
            }
            else if (it.TokenKind == LuaTokenKind.TkColon)
            {
                foundColon = true;
            }
        }
    }

    private int _operatorIndex = -1;

    public LuaNameToken? Operator => Tree.GetElement<LuaNameToken>(_operatorIndex);

    private List<int> _paramTypesIndex = [];

    public IEnumerable<LuaDocTypeSyntax> ParamTypes => Tree.GetElements<LuaDocTypeSyntax>(_paramTypesIndex);

    private int _returnTypeIndex = -1;

    public LuaDocTypeSyntax? ReturnType => Tree.GetElement<LuaDocTypeSyntax>(_returnTypeIndex);
}

public class LuaDocTagMetaSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree);

public class LuaDocTagModuleSyntax : LuaDocTagSyntax
{
    public LuaDocTagModuleSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkString)
            {
                _moduleIndex = it.Index;
            }
            else if (it.TokenKind == LuaTokenKind.TkName)
            {
                _actionIndex = it.Index;
            }
        }
    }

    private int _moduleIndex = -1;

    public LuaStringToken? Module => Tree.GetElement<LuaStringToken>(_moduleIndex);

    private int _actionIndex = -1;

    public LuaNameToken? Action => Tree.GetElement<LuaNameToken>(_actionIndex);
}

public class LuaDocTagMappingSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaNameToken? Name => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();
}

public class LuaDocAttributeSyntax : LuaDocTagSyntax
{
    public LuaDocAttributeSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkName)
            {
                _attributesIndex.Add(it.Index);
            }
        }
    }

    private List<int> _attributesIndex = [];

    public IEnumerable<LuaNameToken> Attributes => Tree.GetElements<LuaNameToken>(_attributesIndex);
}

public class LuaDocTagNamespaceSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaNameToken? Namespace => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();
}

public class LuaDocTagUsingSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaNameToken? Using => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();
}

public class LuaDocTagSourceSyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree)
{
    public LuaStringToken? Source => Iter.FirstChildToken(LuaTokenKind.TkString).ToToken<LuaStringToken>();
}

public class LuaDocTagReadonlySyntax(int index, LuaSyntaxTree tree) : LuaDocTagSyntax(index, tree);
