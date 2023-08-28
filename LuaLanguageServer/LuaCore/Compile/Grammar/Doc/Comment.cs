using LuaLanguageServer.LuaCore.Compile.Grammar.Lua;
using LuaLanguageServer.LuaCore.Compile.Parser;
using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Compile.Grammar.Doc;

public static class CommentParser
{
    public static CompleteMarker Comment(LuaDocParser p, bool topLevel = false)
    {
        var m = p.Marker();

        // do
        // {
        //     StatementParser.Statements(p);
        //     if (!topLevel)
        //     {
        //         break;
        //     }
        // } while (p.Current is not LuaTokenKind.TkEof);

        return m.Complete(p, LuaSyntaxKind.Comment);
    }
}
