using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Common;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.SemanticToken;

// ReSharper disable once ClassNeverInstantiated.Global
public class SemanticTokenHandler(ServerContext context) : SemanticTokensHandlerBase
{
    // private SemanticTokensAnalyzer Analyzer { get; } = new();

    // protected override SemanticTokensRegistrationOptions CreateRegistrationOptions(SemanticTokensCapability capability,
    //     ClientCapabilities clientCapabilities)
    // {
    //     return new()
    //     {
    //         Legend = Analyzer.Legend,
    //         Range = true,
    //         Full = true,
    //     };
    // }
    //
    // protected override Task Tokenize(SemanticTokensBuilder builder, ITextDocumentIdentifierParams identifier,
    //     CancellationToken cancellationToken)
    // {
    //     var uri = identifier.TextDocument.Uri.ToUri().AbsoluteUri;
    //     context.ReadyRead(() =>
    //     {
    //         var semanticModel = context.LuaWorkspace.Compilation.GetSemanticModel(uri);
    //         if (semanticModel is not null)
    //         {
    //             Analyzer.Tokenize(builder, semanticModel, cancellationToken);
    //         }
    //     });
    //     
    //     return Task.CompletedTask;
    // }
    //
    // protected override Task<SemanticTokensDocument> GetSemanticTokensDocument(ITextDocumentIdentifierParams @params,
    //     CancellationToken cancellationToken)
    // {
    //     return Task.FromResult(new SemanticTokensDocument(Analyzer.Legend));
    // }
    protected override Task<SemanticTokens?> Handle(SemanticTokensParams semanticTokensParams, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<SemanticTokensDeltaResponse?> Handle(SemanticTokensDeltaParams semanticTokensDeltaParams, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override Task<SemanticTokens?> Handle(SemanticTokensRangeParams semanticTokensRangeParams, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.SemanticTokensProvider = new SemanticTokensOptions()
        {
            Legend = new SemanticTokensLegend(),
            Full = true,
            Range = true
        };
    }
}