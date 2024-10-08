using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Compile.Parser;

namespace EmmyLua.CodeAnalysis.Compile.Grammar.Doc;

public static class MappedParser
{
    // { [P in keyof T as P]: T[P] }
    // { [P in keyof T]: T[P] }
    public static CompleteMarker TryMappedType(LuaDocParser p)
    {
        var m = p.Marker();
        try
        {
            if (!MappedKeys(p).IsComplete)
            {
                return m.Fail(p, LuaSyntaxKind.TypeMapped, "");
            }
            p.Expect(LuaTokenKind.TkColon);
            TypesParser.Type(p);
            return m.Complete(p, LuaSyntaxKind.TypeMapped);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.TypeMapped, e.Message);
        }
    }

    private static CompleteMarker MappedKeys(LuaDocParser p)
    {
        var m = p.Marker();
        try
        {
            p.Expect(LuaTokenKind.TkLeftBracket);
            TypesParser.Type(p);
            p.Expect(LuaTokenKind.TkRightBracket);
            // [P in keyof T]?
            // [P in keyof T]-?
            if (p.Current is LuaTokenKind.TkMinus)
            {
                p.Bump();
                p.Expect(LuaTokenKind.TkDocQuestion);
            }
            else if (p.Current is LuaTokenKind.TkDocQuestion)
            {
                p.Bump();
            }

            return m.Complete(p, LuaSyntaxKind.TypeMappedKeys);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.TypeMappedKeys, e.Message);
        }
    }
}
