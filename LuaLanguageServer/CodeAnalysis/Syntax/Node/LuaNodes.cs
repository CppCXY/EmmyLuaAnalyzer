using LuaLanguageServer.LuaCore.Syntax.Green;

namespace LuaLanguageServer.LuaCore.Syntax.Node;

public class LuaSourceSyntax : LuaSyntaxNode
{
    public LuaSourceSyntax(GreenNode greenNode)
        : base(greenNode)
    {
    }
}

public class LuaBlockSyntax : LuaSyntaxNode
{
    public LuaBlockSyntax(GreenNode greenNode)
        : base(greenNode)
    {
    }
}

public class LuaStatementSyntax : LuaSyntaxNode
{
    public LuaStatementSyntax(GreenNode greenNode)
        : base(greenNode)
    {
    }
}
