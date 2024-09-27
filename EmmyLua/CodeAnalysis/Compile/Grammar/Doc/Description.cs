﻿using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Compile.Lexer;
using EmmyLua.CodeAnalysis.Compile.Parser;

namespace EmmyLua.CodeAnalysis.Compile.Grammar.Doc;

public static class DescriptionParser
{
    // DocDetail ::= ('#' | '@')? content
    public static CompleteMarker Description(LuaDocParser p)
    {
        if (p.Current is LuaTokenKind.TkEof)
        {
            return CompleteMarker.Empty;
        }
        p.SetState(LuaDocLexerState.Description);
        var m = p.Marker();

        if (p.Current is LuaTokenKind.TkDocDetail)
        {
            p.Bump();
        }
        else
        {
            p.ReCalcCurrent();
        }

        while (p.Current is LuaTokenKind.TkDocDetail)
        {
            p.Bump();
        }

        if (m.IsInvalid(p))
        {
            return CompleteMarker.Empty;
        }

        return m.Complete(p, LuaSyntaxKind.Description);
    }

    public static CompleteMarker InlineDescription(LuaDocParser p)
    {
        if (p.Current is LuaTokenKind.TkEof)
        {
            return CompleteMarker.Empty;
        }
        var m = p.Marker();

        if (p.Current is LuaTokenKind.TkDocDetail)
        {
            p.Bump();
        }

        return m.Complete(p, LuaSyntaxKind.Description);
    }
}
