using EmmyLua.CodeAnalysis.Workspace;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.SemanticToken;

// ReSharper disable once ClassNeverInstantiated.Global
public class SemanticTokenHandler(LuaWorkspace workspace) : SemanticTokensHandlerBase
{
    private SemanticTokensAnalyzer Analyzer { get; } = new();
    
    protected override SemanticTokensRegistrationOptions CreateRegistrationOptions(SemanticTokensCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new()
        {
            DocumentSelector = new TextDocumentSelector(new TextDocumentFilter()
            {
                Pattern = "**/*.lua"
            }),
            Legend = Analyzer.Legend,
            Range = true,
            Full = true,
        };
    }

    protected override Task Tokenize(SemanticTokensBuilder builder, ITextDocumentIdentifierParams identifier,
        CancellationToken cancellationToken)
    {
        var uri = identifier.TextDocument.Uri.ToUnencodedString();
        var semanticModel = workspace.Compilation.GetSemanticModel(uri);
        if (semanticModel is not null)
        {
            Analyzer.Tokenize(builder, semanticModel, cancellationToken);
        }
        return Task.CompletedTask;
    }

    protected override Task<SemanticTokensDocument> GetSemanticTokensDocument(ITextDocumentIdentifierParams @params, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SemanticTokensDocument(Analyzer.Legend));
    }
}