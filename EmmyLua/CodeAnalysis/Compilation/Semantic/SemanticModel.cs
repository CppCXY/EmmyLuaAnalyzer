using EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic;

public class SemanticModel(LuaCompilation compilation, LuaDocument document)
{
    public LuaCompilation Compilation { get; } = compilation;

    public LuaDocument Document { get; } = document;

    public Declaration? GetDeclaration(LuaSyntaxElement element)
    {
        var declarationTree = Compilation.GetDeclarationTree(Document.Id);
        if (declarationTree?.FindDeclaration(element) is { } declaration)
        {
            return declaration;
        }

        return null;
    }
}
