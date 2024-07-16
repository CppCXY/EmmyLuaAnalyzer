using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Common;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.SemanticToken;

public class SemanticTokenHandler(ServerContext context) : SemanticTokensHandlerBase
{
    private SemanticTokensAnalyzer Analyzer { get; } = new();

    protected override Task<SemanticTokens?> Handle(SemanticTokensParams semanticTokensParams,
        CancellationToken cancellationToken)
    {
        var uri = semanticTokensParams.TextDocument.Uri.UnescapeUri;
        SemanticTokens? semanticTokens = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.LuaWorkspace.Compilation.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                semanticTokens = new()
                {
                    Data = Analyzer.Tokenize(semanticModel, context.IsVscode, cancellationToken)
                };
            }
        });

        return Task.FromResult(semanticTokens);
    }

    protected override Task<SemanticTokensDeltaResponse?> Handle(SemanticTokensDeltaParams semanticTokensDeltaParams,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<SemanticTokens?> Handle(SemanticTokensRangeParams semanticTokensRangeParams,
        CancellationToken cancellationToken)
    {
        var uri = semanticTokensRangeParams.TextDocument.Uri.UnescapeUri;
        SemanticTokens? semanticTokens = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.LuaWorkspace.Compilation.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                semanticTokens = new()
                {
                    Data = Analyzer.TokenizeByRange(semanticModel, context.IsVscode, semanticTokensRangeParams.Range, cancellationToken)
                };
            }
        });

        return Task.FromResult(semanticTokens);
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
        if (clientCapabilities.TextDocument?.SemanticTokens?.MultilineTokenSupport.HasValue is true)
        {
            Analyzer.MultiLineTokenSupport = clientCapabilities.TextDocument.SemanticTokens.MultilineTokenSupport.Value;
        }

        serverCapabilities.SemanticTokensProvider = new SemanticTokensOptions()
        {
            Legend = Analyzer.Legend,
            Full = true,
            Range = true
        };
    }
}