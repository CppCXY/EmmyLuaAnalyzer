using LuaLanguageServer.CodeAnalysis.Syntax.Green;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node;

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
