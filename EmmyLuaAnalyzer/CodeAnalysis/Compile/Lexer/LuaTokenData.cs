using EmmyLuaAnalyzer.CodeAnalysis.Compile.Source;
using EmmyLuaAnalyzer.CodeAnalysis.Kind;

namespace EmmyLuaAnalyzer.CodeAnalysis.Compile.Lexer;

public struct LuaTokenData(LuaTokenKind kind, SourceRange range)
{
    public LuaTokenKind Kind { get; } = kind;
    public SourceRange Range { get; } = range;
}
