using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;

public class StubIndexImpl
{
    public LuaCompilation Compilation { get; }

    public StubIndex<string, ILuaSymbol> ShortNameIndex { get; } = new();

    public StubIndex<ILuaType, ILuaSymbol> Members { get; } = new();

    public StubIndex<string, ILuaNamedType> LuaTypeIndex { get; } = new();

    public StubIndex<LuaSyntaxElement, ILuaSymbol> SyntaxIndex { get; } = new();

    public StubIndexImpl(LuaCompilation compilation)
    {
        Compilation = compilation;
    }
}
