using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaDocTypeSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public static bool CanCast(LuaSyntaxKind kind) => kind is >= LuaSyntaxKind.TypeArray and <= LuaSyntaxKind.TypeMatch;

    public LuaDescriptionSyntax? Description =>
        Iter.FirstChildNode(LuaSyntaxKind.Description).ToNode<LuaDescriptionSyntax>();
}

public class LuaDocLiteralTypeSyntax : LuaDocTypeSyntax
{
    public LuaDocLiteralTypeSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            switch (it.TokenKind)
            {
                case LuaTokenKind.TkInt:
                    IsInteger = true;
                    _literalIndex = it.Index;
                    break;
                case LuaTokenKind.TkString:
                    IsString = true;
                    _literalIndex = it.Index;
                    break;
                case LuaTokenKind.TkDocBoolean:
                    IsBoolean = true;
                    _literalIndex = it.Index;
                    break;
            }
        }
    }

    public bool IsInteger { get; }

    public bool IsString { get; }

    public bool IsBoolean { get; }

    private int _literalIndex = -1;

    public LuaIntegerToken? Integer => IsInteger ? Tree.GetElement<LuaIntegerToken>(_literalIndex) : null;

    public LuaStringToken? String => IsString ? Tree.GetElement<LuaStringToken>(_literalIndex) : null;

    public LuaSyntaxToken? Boolean => IsBoolean ? Tree.GetElement<LuaSyntaxToken>(_literalIndex) : null;
}

public class LuaDocNameTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaNameToken? Name => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();
}

public class LuaDocArrayTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocTypeSyntax? BaseType => Iter.FirstChildNode(CanCast).ToNode<LuaDocTypeSyntax>();
}

public class LuaDocObjectTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocBodySyntax? Body => Iter.FirstChildNode(LuaSyntaxKind.DocBody).ToNode<LuaDocBodySyntax>();
}

public class LuaDocTypedParamSyntax : LuaSyntaxNode
{
    public LuaDocTypedParamSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkDots)
            {
                IsVarArgs = true;
                _nameIndex = it.Index;
                break;
            }
            else if (it.TokenKind == LuaTokenKind.TkName)
            {
                _nameIndex = it.Index;
            }
            else if (it.TokenKind == LuaTokenKind.TkDocQuestion)
            {
                Nullable = true;
            }
            else if (LuaDocTypeSyntax.CanCast(it.Kind))
            {
                _typeIndex = it.Index;
            }
        }
    }

    private int _nameIndex = -1;

    public LuaNameToken? Name => !IsVarArgs ? Tree.GetElement<LuaNameToken>(_nameIndex) : null;

    public LuaDotsToken? VarArgs => IsVarArgs ? Tree.GetElement<LuaDotsToken>(_nameIndex) : null;

    private int _typeIndex = -1;

    public LuaDocTypeSyntax? Type => Tree.GetElement<LuaDocTypeSyntax>(_typeIndex);

    public bool Nullable { get; }

    public bool IsVarArgs { get; }
}

public class LuaDocFuncTypeSyntax : LuaDocTypeSyntax
{
    public LuaDocFuncTypeSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.Kind == LuaSyntaxKind.TypedParameter)
            {
                _paramListIndex.Add(it.Index);
            }
            else if (CanCast(it.Kind))
            {
                _returnTypeIndex.Add(it.Index);
            }
        }
    }

    private List<int> _paramListIndex = [];

    public IEnumerable<LuaDocTypedParamSyntax> ParamList => Tree.GetElements<LuaDocTypedParamSyntax>(_paramListIndex);

    private List<int> _returnTypeIndex = [];

    public IEnumerable<LuaDocTypeSyntax> ReturnType => Tree.GetElements<LuaDocTypeSyntax>(_returnTypeIndex);
}

public class LuaDocUnionTypeSyntax : LuaDocTypeSyntax
{
    public LuaDocUnionTypeSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (CanCast(it.Kind))
            {
                _unionTypeIndex.Add(it.Index);
            }
        }
    }

    private List<int> _unionTypeIndex = [];

    public IEnumerable<LuaDocTypeSyntax> UnionTypes => Tree.GetElements<LuaDocTypeSyntax>(_unionTypeIndex);
}

public class LuaDocTupleTypeSyntax : LuaDocTypeSyntax
{
    public LuaDocTupleTypeSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (CanCast(it.Kind))
            {
                _tupleTypeIndex.Add(it.Index);
            }
        }
    }

    private List<int> _tupleTypeIndex = [];

    public IEnumerable<LuaDocTypeSyntax> TypeList => Tree.GetElements<LuaDocTypeSyntax>(_tupleTypeIndex);
}

public class LuaDocParenTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocTypeSyntax? Type => Iter.FirstChildNode(CanCast).ToNode<LuaDocTypeSyntax>();
}

public class LuaDocGenericTypeSyntax : LuaDocTypeSyntax
{
    public LuaDocGenericTypeSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkName)
            {
                _nameIndex = it.Index;
            }
            else if (CanCast(it.Kind))
            {
                _genericTypeListIndex.Add(it.Index);
            }
        }
    }

    private int _nameIndex = -1;

    public LuaNameToken? Name => Tree.GetElement<LuaNameToken>(_nameIndex);

    private List<int> _genericTypeListIndex = [];

    public IEnumerable<LuaDocTypeSyntax> GenericArgs => Tree.GetElements<LuaDocTypeSyntax>(_genericTypeListIndex);
}

public class LuaDocVariadicTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaNameToken? Name => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();
}

public class LuaDocExpandTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaNameToken? Name => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();
}

public class LuaDocStringTemplateTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaNameToken? PrefixName => Iter.FirstChildToken(LuaTokenKind.TkName).ToToken<LuaNameToken>();

    public LuaTemplateTypeToken? TemplateName =>
        Iter.FirstChildToken(LuaTokenKind.TkStringTemplateType).ToToken<LuaTemplateTypeToken>();
}

public class LuaDocKeyOfTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaSyntaxToken KeyOfToken => Iter.FirstChildToken(LuaTokenKind.TkDocKeyOf).ToToken<LuaSyntaxToken>()!;

    public LuaDocTypeSyntax? Type => Iter.FirstChildNode(CanCast).ToNode<LuaDocTypeSyntax>();
}

public class LuaDocConditionalTypeSyntax : LuaDocTypeSyntax
{
    public LuaDocConditionalTypeSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        var typeCount = 0;
        foreach (var it in Iter.Children)
        {
            if (it.TokenKind == LuaTokenKind.TkDocQuestion)
            {
                _questionIndex = it.Index;
            }
            else if (CanCast(it.Kind))
            {
                switch (typeCount)
                {
                    case 0:
                        _checkTypeIndex = it.Index;
                        break;
                    case 1:
                        _trueTypeIndex = it.Index;
                        break;
                    case 2:
                        _falseTypeIndex = it.Index;
                        break;
                }

                typeCount++;
            }
        }
    }

    private int _questionIndex = -1;

    public LuaSyntaxToken QuestionToken => Tree.GetElement<LuaSyntaxToken>(_questionIndex)!;

    private int _checkTypeIndex = -1;

    public LuaDocTypeSyntax? CheckType => Tree.GetElement<LuaDocTypeSyntax>(_checkTypeIndex);

    private int _trueTypeIndex = -1;

    public LuaDocTypeSyntax? TrueType => Tree.GetElement<LuaDocTypeSyntax>(_trueTypeIndex);

    private int _falseTypeIndex = -1;

    public LuaDocTypeSyntax? FalseType => Tree.GetElement<LuaDocTypeSyntax>(_falseTypeIndex);
}

public class LuaDocMappedTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocMappedKeysSyntax? KeyType =>
        Iter.FirstChildNode(LuaSyntaxKind.TypeMappedKeys).ToNode<LuaDocMappedKeysSyntax>();

    public LuaDocTypeSyntax? ValueType => Iter.FirstChildNode(CanCast).ToNode<LuaDocTypeSyntax>();
}

public class LuaDocMappedKeysSyntax : LuaDocTypeSyntax
{
    public LuaDocMappedKeysSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (CanCast(it.Kind))
            {
                _iterTypeIndex = it.Index;
            }
            else if (it.TokenKind == LuaTokenKind.TkMinus)
            {
                IsMinus = true;
            }
            else if (it.TokenKind == LuaTokenKind.TkDocQuestion)
            {
                HasNullable = true;
            }
        }
    }

    private int _iterTypeIndex = -1;

    public LuaDocTypeSyntax? IterType => Tree.GetElement<LuaDocTypeSyntax>(_iterTypeIndex);

    public bool IsMinus { get; }

    public bool HasNullable { get; }
}

public class LuaDocIndexAccessTypeSyntax : LuaDocTypeSyntax
{
    public LuaDocIndexAccessTypeSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (CanCast(it.Kind))
            {
                if (_baseTypeIndex == -1)
                {
                    _baseTypeIndex = it.Index;
                }
                else
                {
                    _indexTypeIndex = it.Index;
                }
            }
        }
    }

    private int _baseTypeIndex = -1;

    public LuaDocTypeSyntax? BaseType => Tree.GetElement<LuaDocTypeSyntax>(_baseTypeIndex);

    private int _indexTypeIndex = -1;

    public LuaDocTypeSyntax? IndexType => Tree.GetElement<LuaDocTypeSyntax>(_indexTypeIndex);
}

public class LuaDocInTypeSyntax : LuaDocTypeSyntax
{
    public LuaDocInTypeSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (CanCast(it.Kind))
            {
                if (_keyTypeIndex == -1)
                {
                    _keyTypeIndex = it.Index;
                }
                else
                {
                    _indexTypeIndex = it.Index;
                }
            }
            else if (it.TokenKind == LuaTokenKind.TkDocIn)
            {
                _inTypeIndex = it.Index;
            }
        }
    }

    private int _inTypeIndex = -1;

    public LuaSyntaxToken InToken => Tree.GetElement<LuaSyntaxToken>(_inTypeIndex)!;

    private int _keyTypeIndex = -1;

    public LuaDocNameTypeSyntax? KeyType => Tree.GetElement<LuaDocNameTypeSyntax>(_keyTypeIndex);

    private int _indexTypeIndex = -1;

    public LuaDocTypeSyntax? IndexType => Tree.GetElement<LuaDocTypeSyntax>(_indexTypeIndex);
}

public class LuaDocExtendTypeSyntax : LuaDocTypeSyntax
{
    public LuaDocExtendTypeSyntax(int index, LuaSyntaxTree tree) : base(index, tree)
    {
        foreach (var it in Iter.Children)
        {
            if (CanCast(it.Kind))
            {
                if (_baseTypeIndex == -1)
                {
                    _baseTypeIndex = it.Index;
                }
                else
                {
                    _extendTypeIndex = it.Index;
                }
            }
            else if (it.TokenKind == LuaTokenKind.TkDocExtends)
            {
                _extendsIndex = it.Index;
            }
        }
    }

    private int _extendsIndex = -1;

    public LuaSyntaxToken ExtendToken => Tree.GetElement<LuaSyntaxToken>(_extendsIndex)!;

    private int _baseTypeIndex = -1;

    public LuaDocTypeSyntax? BaseType => Tree.GetElement<LuaDocTypeSyntax>(_baseTypeIndex);

    private int _extendTypeIndex = -1;

    public LuaDocTypeSyntax? ExtendType => Tree.GetElement<LuaDocTypeSyntax>(_extendTypeIndex);
}

public class LuaDocIntersectionTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public IEnumerable<LuaDocTypeSyntax> IntersectionTypes => Iter.ChildrenNodeOfType<LuaDocTypeSyntax>(CanCast);
}
