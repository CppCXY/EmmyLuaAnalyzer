using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compile.Lexer;

public readonly struct LuaTokenData(LuaTokenKind kind, SourceRange range)
{
    public LuaTokenKind Kind { get; } = kind;
    public SourceRange Range { get; } = range;
}
