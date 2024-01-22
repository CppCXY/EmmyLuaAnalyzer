using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic;

public class SemanticModel(LuaCompilation compilation, LuaDocument document)
{
    public LuaCompilation Compilation { get; } = compilation;

    public LuaDocument Document { get; } = document;

    public Symbol.Symbol? GetSymbol(LuaSyntaxElement element)
    {
        var declarationTree = Compilation.GetSymbolTree(Document.Id);
        if (declarationTree?.FindDeclaration(element) is { } declaration)
        {
            return declaration;
        }

        return null;
    }
}
