using LanguageServer.Server;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.DocumentSymbol;

// ReSharper disable once ClassNeverInstantiated.Global
public class DocumentSymbolHandler(ServerContext context) : DocumentSymbolHandlerBase
{
    private DocumentSymbolBuilder Builder { get; } = new();

    protected override DocumentSymbolRegistrationOptions CreateRegistrationOptions(DocumentSymbolCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new()
        {
            Label = "EmmyLua",
            DocumentSelector = ToSelector.ToTextDocumentSelector(context.LuaWorkspace)
        };
    }

    public override Task<SymbolInformationOrDocumentSymbolContainer?> Handle(DocumentSymbolParams request,
        CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUri().AbsoluteUri;
        SymbolInformationOrDocumentSymbolContainer? container = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var symbols = Builder.Build(semanticModel);
                container = SymbolInformationOrDocumentSymbolContainer.From(
                    symbols.Select(it => new SymbolInformationOrDocumentSymbol(it)));
            }
        });
        
        return Task.FromResult<SymbolInformationOrDocumentSymbolContainer?>(container);
    }
}