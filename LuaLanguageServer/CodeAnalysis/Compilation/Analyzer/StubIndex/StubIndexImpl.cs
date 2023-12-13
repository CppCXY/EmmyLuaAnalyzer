using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.StubIndex;

public class StubIndexImpl(LuaCompilation compilation)
{
    public LuaCompilation Compilation { get; } = compilation;

    public StubIndex<string, ILuaSymbol> ShortNameIndex { get; } = new();

    public StubIndex<ILuaType, ILuaSymbol> Members { get; } = new();

    public StubIndex<string, ILuaNamedType> LuaTypeIndex { get; } = new();

    public StubIndex<LuaSyntaxElement, ILuaSymbol> SyntaxIndex { get; } = new();

    public StubIndex<string, Declaration.Declaration> Global { get; } = new();
}
