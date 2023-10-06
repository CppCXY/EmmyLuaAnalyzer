using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Infer;

public static class DeclarationInfer
{
    public static ILuaSymbol InferLocalName(LuaLocalNameSyntax localName, SearchContext context)
    {
        return localName switch
        {
            // LuaIdentifierLocalNameSyntax identifierLocalName => InferIdentifierLocalName(identifierLocalName),
            _ => throw new NotImplementedException()
        };
    }

    public static ILuaSymbol InferSource(LuaSourceSyntax source, SearchContext context)
    {
        return source switch
        {
            // LuaChunkSyntax chunk => InferChunk(chunk, context),
            _ => throw new NotImplementedException()
        };
    }

    public static ILuaSymbol InferParam(LuaParamDefSyntax paramDef, SearchContext context)
    {
        return paramDef switch
        {
            // LuaIdentifierParamDefSyntax identifierParamDef => InferIdentifierParamDef(identifierParamDef),
            _ => throw new NotImplementedException()
        };
    }
}
