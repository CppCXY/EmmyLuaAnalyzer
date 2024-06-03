using EmmyLua.CodeAnalysis.Syntax.Kind;
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

    public LuaIntegerToken? Integer => FirstChild<LuaIntegerToken>();

    public LuaStringToken? String => FirstChild<LuaStringToken>();
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

public class LuaDocAggregateTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public IEnumerable<LuaDocTypeSyntax> TypeList => ChildrenElement<LuaDocTypeSyntax>();
}

public class LuaDocTemplateTypeSyntax(int index, LuaSyntaxTree tree) : LuaDocTypeSyntax(index, tree)
{
    public LuaTemplateTypeToken? TemplateName => FirstChild<LuaTemplateTypeToken>();
}
