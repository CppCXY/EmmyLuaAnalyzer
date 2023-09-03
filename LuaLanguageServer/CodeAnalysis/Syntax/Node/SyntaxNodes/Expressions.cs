using LuaLanguageServer.CodeAnalysis.Syntax.Green;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class ExpressionSyntax : LuaSyntaxNode
{
    public ExpressionSyntax(GreenNode greenNode)
        : base(greenNode)
    {
    }
}

public class NameSyntax : ExpressionSyntax
{
    public LuaSyntaxToken Name { get; }

    public NameSyntax(GreenNode greenNode, LuaSyntaxToken name)
        : base(greenNode)
    {
        Name = name;
    }
}

public class VarDefSyntax : ExpressionSyntax
{
    public LuaSyntaxToken Name { get; }

    public ExpressionSyntax? Expression { get; }

    public VarDefSyntax(GreenNode greenNode, LuaSyntaxToken name, ExpressionSyntax? expression)
        : base(greenNode)
    {
        Name = name;
        Expression = expression;
    }
}
