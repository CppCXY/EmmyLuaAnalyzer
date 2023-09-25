using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Declaration;

public interface IDeclaration
{
    public string Name { get; }

    public LuaSyntaxElement SyntaxElement { get; }
}
