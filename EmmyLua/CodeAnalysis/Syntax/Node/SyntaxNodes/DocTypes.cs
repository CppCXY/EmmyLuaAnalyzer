using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaDocTypeSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public LuaDescriptionSyntax? Description => FirstChild<LuaDescriptionSyntax>();
}

public class LuaDocLiteralTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public bool IsInteger => FirstChild<LuaIntegerToken>() != null;

    public bool IsString => FirstChild<LuaStringToken>() != null;

    public bool IsBoolean => FirstChildToken(LuaTokenKind.TkDocBoolean) != null;

    public LuaIntegerToken? Integer => FirstChild<LuaIntegerToken>();

    public LuaStringToken? String => FirstChild<LuaStringToken>();

    public LuaSyntaxToken? Boolean => FirstChildToken(LuaTokenKind.TkDocBoolean);
}

public class LuaDocNameTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaDocArrayTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocTypeSyntax? BaseType => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocObjectTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocBodySyntax? Body => FirstChild<LuaDocBodySyntax>();
}

public class LuaDocTypedParamSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDotsToken? VarArgs => FirstChild<LuaDotsToken>();

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public bool Nullable => FirstChildToken(LuaTokenKind.TkDocQuestion) != null;

    public bool IsVarArgs => VarArgs != null;
}

public class LuaDocFuncTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public IEnumerable<LuaDocTypedParamSyntax> ParamList => ChildrenElement<LuaDocTypedParamSyntax>();

    public IEnumerable<LuaDocTypeSyntax> ReturnType => ChildrenElement<LuaDocTypeSyntax>();
}

public class LuaDocUnionTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public IEnumerable<LuaDocTypeSyntax> UnionTypes => ChildrenElement<LuaDocTypeSyntax>();
}

public class LuaDocTupleTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public IEnumerable<LuaDocTypeSyntax> TypeList => ChildrenElement<LuaDocTypeSyntax>();
}

public class LuaDocParenTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocGenericTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public IEnumerable<LuaDocTypeSyntax> GenericArgs => ChildrenElement<LuaDocTypeSyntax>();
}

public class LuaDocVariadicTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaDocExpandTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();
}

public class LuaDocStringTemplateTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaNameToken? PrefixName => FirstChild<LuaNameToken>();

    public LuaTemplateTypeToken? TemplateName => FirstChild<LuaTemplateTypeToken>();
}

public class LuaDocKeyOfTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocConditionalTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaSyntaxToken QuestionToken => FirstChildToken(LuaTokenKind.TkDocQuestion)!;

    public LuaDocTypeSyntax? CheckType => FirstChild<LuaDocTypeSyntax>();

    public LuaDocTypeSyntax? TrueType => ChildrenElements.OfType<LuaDocTypeSyntax>().Skip(1).FirstOrDefault();

    public LuaDocTypeSyntax? FalseType => ChildrenElements.OfType<LuaDocTypeSyntax>().Skip(2).FirstOrDefault();
}

public class LuaDocMappedTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocMappedKeysSyntax? Key => FirstChild<LuaDocMappedKeysSyntax>();

    public LuaDocTypeSyntax? ValueType => ChildrenElements.OfType<LuaDocTypeSyntax>().Skip(1).FirstOrDefault();
}

public class LuaDocMappedKeysSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDocTypeSyntax? IterType => FirstChild<LuaDocTypeSyntax>();

    // other TODO
}

public class LuaDocIndexAccessTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocTypeSyntax? BaseType => FirstChild<LuaDocTypeSyntax>();

    public LuaDocTypeSyntax? IndexType => ChildrenElements.OfType<LuaDocTypeSyntax>().Skip(1).FirstOrDefault();
}

public class LuaDocInTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocNameTypeSyntax? KeyType => FirstChild<LuaDocNameTypeSyntax>();

    public LuaDocTypeSyntax? IndexType => ChildrenElements.OfType<LuaDocTypeSyntax>().Skip(1).FirstOrDefault();
}

public class LuaDocExtendTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaSyntaxToken ExtendToken => FirstChildToken(LuaTokenKind.TkDocExtends)!;

    public LuaDocTypeSyntax? BaseType => FirstChild<LuaDocTypeSyntax>();

    public LuaDocTypeSyntax? ExtendType => ChildrenElements.OfType<LuaDocTypeSyntax>().Skip(1).FirstOrDefault();
}

public class LuaDocIntersectionTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public IEnumerable<LuaDocTypeSyntax> IntersectionTypes => ChildrenElement<LuaDocTypeSyntax>();
}
