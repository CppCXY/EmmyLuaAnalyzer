using EmmyLua.CodeAnalysis.Compile.Parser;
using EmmyLua.CodeAnalysis.Syntax.Kind;

namespace EmmyLua.CodeAnalysis.Compile.Grammar.Lua;

public static class BlockParser
{
    public static CompleteMarker Block(LuaParser p, bool topLevel = false)
    {
        var m = p.Marker();

        do
        {
            StatementParser.Statements(p);
            if (!topLevel)
            {
                break;
            }
        } while (p.Current is not LuaTokenKind.TkEof);

        return m.Complete(p, LuaSyntaxKind.Block);
    }
}
