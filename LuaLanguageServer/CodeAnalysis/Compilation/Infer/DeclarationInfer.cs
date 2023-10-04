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
}
