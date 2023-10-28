using System.Collections;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaDocTypeSyntax : LuaSyntaxElement
{
    public LuaSyntaxToken? Description => FirstChildToken(LuaTokenKind.TkDocDescription);

    public LuaDocTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocLiteralTypeSyntax : LuaDocTypeSyntax
{
    public bool IsInteger => FirstChildToken(LuaTokenKind.TkInt) != null;

    public bool IsString => FirstChildToken(LuaTokenKind.TkString) != null;

    public string RepresentText
    {
        get
        {
            if (IsInteger)
            {
                return FirstChildToken(LuaTokenKind.TkInt)!.RepresentText;
            }
            else if (IsString)
            {
                return FirstChildToken(LuaTokenKind.TkString)!.RepresentText;
            }
            else
            {
                return "";
            }
        }
    }

    public LuaDocLiteralTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocNameTypeSyntax : LuaDocTypeSyntax
{
    public LuaSyntaxToken? Name => FirstChildToken(LuaTokenKind.TkName);

    public LuaDocNameTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocArrayTypeSyntax : LuaDocTypeSyntax
{
    public LuaDocTypeSyntax? BaseType => FirstChild<LuaDocTypeSyntax>();

    public LuaDocArrayTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTableTypeSyntax : LuaDocTypeSyntax
{
    public IEnumerable<LuaDocTypedFieldSyntax> FieldList => ChildNodes<LuaDocTypedFieldSyntax>();

    public LuaDocTableTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocFuncTypeSyntax : LuaDocTypeSyntax
{
    public IEnumerable<LuaDocTypedParamSyntax> ParamList => ChildNodes<LuaDocTypedParamSyntax>();

    public LuaDocTypeSyntax? ReturnType => FirstChild<LuaDocTypeSyntax>();

    public LuaDocFuncTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocUnionTypeSyntax : LuaDocTypeSyntax
{
    public IEnumerable<LuaDocTypeSyntax> UnionTypes => ChildNodes<LuaDocTypeSyntax>();

    public LuaDocUnionTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTupleTypeSyntax : LuaDocTypeSyntax
{
    public IEnumerable<LuaDocTypeSyntax> TypeList => ChildNodes<LuaDocTypeSyntax>();

    public LuaDocTupleTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocParenTypeSyntax : LuaDocTypeSyntax
{
    public LuaDocTypeSyntax? Type => FirstChild<LuaDocTypeSyntax>();

    public LuaDocParenTypeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaDocTypedFieldSyntax : LuaDocSyntax
{
    public bool IsNameKey => FirstChildToken(LuaTokenKind.TkName) != null;

    public bool IsStringKey => FirstChildToken(LuaTokenKind.TkString) != null;

    public bool IsIntegerKey => FirstChildToken(LuaTokenKind.TkInt) != null;

    public bool IsTypeKey => ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon).FirstOrDefault() != null;

    public LuaNameToken? NameKey => FirstChild<LuaNameToken>();

    public LuaStringToken? StringKey => FirstChild<LuaStringToken>();

    public LuaNumberToken? IntegerKey => FirstChild<LuaNumberToken>();

    public LuaDocTypeSyntax? TypeKey => ChildNodesBeforeToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon).FirstOrDefault();

    public LuaDocTypeSyntax? Type => ChildNodesAfterToken<LuaDocTypeSyntax>(LuaTokenKind.TkColon).FirstOrDefault();

    public LuaDocTypedFieldSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }
}
