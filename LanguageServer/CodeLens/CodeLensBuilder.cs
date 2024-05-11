using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LanguageServer.Server;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.CodeLens;

public class CodeLensBuilder
{
    public CodeLensContainer Build(SemanticModel semanticModel, ServerContext context)
    {
        var codeLens = new List<OmniSharp.Extensions.LanguageServer.Protocol.Models.CodeLens>();
        var funcStats = semanticModel.Document
            .SyntaxTree
            .SyntaxRoot
            .Descendants
            .OfType<LuaFuncStatSyntax>();

        var document = semanticModel.Document;
        foreach (var funcStat in funcStats)
        {
            if (funcStat.FirstChildToken(LuaTokenKind.TkFunction) is { } funcToken)
            {
                codeLens.Add(new OmniSharp.Extensions.LanguageServer.Protocol.Models.CodeLens()
                {
                    Range = funcToken.Range.ToLspRange(document),
                    // Command = 
                });
            }

        }
        
        
        return new CodeLensContainer(codeLens);
    }
}