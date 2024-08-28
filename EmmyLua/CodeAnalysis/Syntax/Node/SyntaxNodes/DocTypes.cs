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

public class LuaDocTableTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocBodySyntax? Body => FirstChild<LuaDocBodySyntax>();
}

public class LuaDocTypedParamSyntax(int index, LuaSyntaxTree tree) : LuaSyntaxNode(index, tree)
{
    public LuaNameToken? Name => FirstChild<LuaNameToken>();

    public LuaDotsToken? VarArgs => FirstChild<LuaDotsToken>();

    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public bool Nullable => FirstChildToken(LuaTokenKind.TkNullable) != null;

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

public class LuaDocTemplateTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaNameToken? PrefixName => FirstChild<LuaNameToken>();

    public LuaTemplateTypeToken? TemplateName => FirstChild<LuaTemplateTypeToken>();
}

public class LuaDocKeyOfTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocTypeOfTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();
}

public class LuaDocConditionalTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocTypeSyntax? CheckType => FirstChild<LuaDocTypeSyntax>();

    public LuaDocTypeSyntax? TrueType => ChildrenElements.OfType<LuaDocTypeSyntax>().Skip(1).FirstOrDefault();

    public LuaDocTypeSyntax? FalseType => ChildrenElements.OfType<LuaDocTypeSyntax>().Skip(2).FirstOrDefault();
}

public class LuaDocMappedTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocTypeSyntax? KeyType => FirstChild<LuaDocTypeSyntax>();

    public LuaDocTypeSyntax? ValueType => ChildrenElements.OfType<LuaDocTypeSyntax>().Skip(1).FirstOrDefault();
}

public class LuaDocIndexAccessTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocTypeSyntax? BaseType => FirstChild<LuaDocTypeSyntax>();

    public LuaDocTypeSyntax? IndexType => ChildrenElements.OfType<LuaDocTypeSyntax>().Skip(1).FirstOrDefault();
}

public class LuaDocInTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaDocNameTypeSyntax? BaseType => FirstChild<LuaDocNameTypeSyntax>();

    public LuaDocTypeSyntax? IndexType => ChildrenElements.OfType<LuaDocTypeSyntax>().Skip(1).FirstOrDefault();
}
