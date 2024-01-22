using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Kind;

namespace EmmyLua.CodeAnalysis.Compile.Lexer;

public struct LuaTokenData(LuaTokenKind kind, SourceRange range)
{
    public LuaTokenKind Kind { get; } = kind;
    public SourceRange Range { get; } = range;
}
