using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;

public static class DeclarationInfer
{
    public static DeclarationTree? GetDeclarationTree(LuaSyntaxElement element, SearchContext context)
    {
        var source = element.Tree.Source;
        if (source is LuaDocument document)
        {
            return context.Compilation.GetDeclarationTree(document.Id);
        }

        return null;
    }

    public static ILuaType InferLocalName(LuaLocalNameSyntax localName, SearchContext context)
    {
        var declarationTree = GetDeclarationTree(localName, context);
        if (declarationTree is null)
        {
            return context.Compilation.Builtin.Unknown;
        }

        var declaration = declarationTree.FindDeclaration(localName);
        return declaration?.FirstDeclaration.Type ?? context.Compilation.Builtin.Unknown;
    }

    public static ILuaType InferSource(LuaSourceSyntax source, SearchContext context)
    {
        return source switch
        {
            // LuaChunkSyntax chunk => InferChunk(chunk, context),
            _ => throw new NotImplementedException()
        };
    }

    public static ILuaType InferParam(LuaParamDefSyntax paramDef, SearchContext context)
    {
        var declarationTree = GetDeclarationTree(paramDef, context);
        if (declarationTree is null)
        {
            return context.Compilation.Builtin.Unknown;
        }

        var declaration = declarationTree.FindDeclaration(paramDef);
        return declaration?.FirstDeclaration.Type ?? context.Compilation.Builtin.Unknown;
    }
}
