using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaSourceSyntax : LuaSyntaxNode
{
    public LuaBlockSyntax? BlockSyntax => FirstChild<LuaBlockSyntax>();

    public LuaSourceSyntax(GreenNode greenNode, LuaSyntaxTree tree)
        : base(greenNode, tree, null)
    {
    }
}

public class LuaBlockSyntax : LuaSyntaxNode
{
    public IEnumerable<LuaStatSyntax> StatementSyntaxList => ChildNodes<LuaStatSyntax>();

    public LuaBlockSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaParamListSyntax : LuaSyntaxNode
{
    public IEnumerable<LuaSyntaxToken> NameList => ChildTokens(LuaTokenKind.TkName);

    public bool HasVarArgs => FirstChildToken(LuaTokenKind.TkDots) != null;

    public LuaParamListSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}

public class LuaAttributeSyntax : LuaSyntaxNode
{
    public LuaSyntaxToken? Name => FirstChildToken(LuaTokenKind.TkName);

    public bool IsConst
    {
        get
        {
            if (Name == null)
            {
                return false;
            }

            return Name.Text == "const";
        }
    }

    public bool IsClose
    {
        get
        {
            if (Name == null)
            {
                return false;
            }

            return Name.Text == "close";
        }
    }

    public LuaAttributeSyntax(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxNode? parent)
        : base(greenNode, tree, parent)
    {
    }
}

