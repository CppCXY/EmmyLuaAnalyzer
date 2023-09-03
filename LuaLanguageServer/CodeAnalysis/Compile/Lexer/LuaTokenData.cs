using LuaLanguageServer.LuaCore.Compile.Source;
using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Compile.Lexer;

public struct LuaTokenData
{
    public LuaTokenKind Kind { get; }
    public SourceRange Range { get; }

    public LuaTokenData(LuaTokenKind kind, SourceRange range)
    {
        Kind = kind;
        Range = range;
    }
}
