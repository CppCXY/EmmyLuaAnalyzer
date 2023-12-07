using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;

public static class DeclarationInfer
{
    public static ILuaType InferLocalName(LuaLocalNameSyntax localName, SearchContext context)
    {
        return localName switch
        {
            // LuaIdentifierLocalNameSyntax identifierLocalName => InferIdentifierLocalName(identifierLocalName),
            _ => throw new NotImplementedException()
        };
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
        return paramDef switch
        {
            // LuaIdentifierParamDefSyntax identifierParamDef => InferIdentifierParamDef(identifierParamDef),
            _ => throw new NotImplementedException()
        };
    }
}
