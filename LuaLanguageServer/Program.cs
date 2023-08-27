using LuaLanguageServer.LuaCore.Compile;
using LuaLanguageServer.LuaCore.Compile.Lexer;
using LuaLanguageServer.LuaCore.Compile.Parser;
using LuaLanguageServer.LuaCore.Compile.Source;

var lang = new LuaLanguage();
var source = LuaSource.From(
    """
    local t= 123
    """, lang);

LuaParser parser = new LuaParser(new LuaLexer(source));
parser.Parse();

Console.Write(parser.Events);
