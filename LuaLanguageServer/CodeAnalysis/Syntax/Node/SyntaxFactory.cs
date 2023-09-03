using LuaLanguageServer.LuaCore.Syntax.Green;
using LuaLanguageServer.LuaCore.Syntax.Tree;

namespace LuaLanguageServer.LuaCore.Syntax.Node;

public class SyntaxFactory
{
    public static LuaSourceSyntax SourceSyntax(GreenNode greenNode)
    {
        // for (int i = 0; i <; i++)
        // {
        //
        // }
        // var blockSyntax = CreateBlock()

        return new LuaSourceSyntax(greenNode);
    }


}
