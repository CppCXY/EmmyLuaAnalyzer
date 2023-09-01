using LuaLanguageServer.LuaCore.Compile;
using LuaLanguageServer.LuaCore.Compile.Lexer;
using LuaLanguageServer.LuaCore.Compile.Parser;
using LuaLanguageServer.LuaCore.Compile.Source;

var lang = new LuaLanguage();
var source = LuaSource.From(
    """
    --- 你说的对但是__
    ---@class A {a :number} # 1231313
    local t= 123
    """, lang);

LuaParser parser = new LuaParser(new LuaLexer(source));
parser.Parse();

Console.Write(parser.Events);
