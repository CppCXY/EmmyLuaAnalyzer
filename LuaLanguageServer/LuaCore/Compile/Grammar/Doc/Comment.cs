using System.Diagnostics;
using LuaLanguageServer.LuaCore.Compile.Grammar.Lua;
using LuaLanguageServer.LuaCore.Compile.Parser;
using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Compile.Grammar.Doc;

public static class CommentParser
{
    public static CompleteMarker Comment(LuaDocParser p)
    {
        var m = p.Marker();

        Docs(p);

        return m.Complete(p, LuaSyntaxKind.Comment);
    }

    private static void Docs(LuaDocParser p)
    {
        while (p.Current is not LuaTokenKind.TkEof)
        {
            switch (p.Current)
            {
                case LuaTokenKind.TkDocStart:
                {
                }
                case LuaTokenKind.TkDocLongStart:
                {
                }
                case LuaTokenKind.TkNormalStart:
                {
                }
                case LuaTokenKind.TkDocTrivia:
                {
                }
                default:
                {
                    throw new UnreachableException();
                }
            }
        }
    }
}
