using LuaLanguageServer.CodeAnalysis.Compile.Source;
using LuaLanguageServer.CodeAnalysis.Kind;

namespace LuaLanguageServer.CodeAnalysis.Compile.Lexer;

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
