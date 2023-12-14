using LuaLanguageServer.CodeAnalysis.Compile.Source;
using LuaLanguageServer.CodeAnalysis.Kind;

namespace LuaLanguageServer.CodeAnalysis.Compile.Lexer;

public struct LuaTokenData(LuaTokenKind kind, SourceRange range)
{
    public LuaTokenKind Kind { get; } = kind;
    public SourceRange Range { get; } = range;
}
