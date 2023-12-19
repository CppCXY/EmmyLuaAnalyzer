using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Compilation.TypeOperator;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Stub;

public class StubIndexImpl(LuaCompilation compilation)
{
    public LuaCompilation Compilation { get; } = compilation;

    public StubIndex<string, ILuaSymbol> ShortNameIndex { get; } = new();

    public StubIndex<string, Declaration.Declaration> Members { get; } = new();

    public StubIndex<string, ILuaNamedType> NamedTypeIndex { get; } = new();

    public StubIndex<LuaSyntaxElement, ILuaSymbol> SyntaxIndex { get; } = new();

    public StubIndex<string, Declaration.Declaration> GlobalDeclaration { get; } = new();

    public StubIndex<string, ILuaOperator> TypeOperators { get; } = new();
}
