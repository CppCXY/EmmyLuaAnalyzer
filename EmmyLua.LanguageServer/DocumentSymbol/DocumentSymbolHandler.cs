using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.DocumentSymbol;

// ReSharper disable once ClassNeverInstantiated.Global
public class DocumentSymbolHandler(ServerContext context) : DocumentSymbolHandlerBase
{
    private DocumentSymbolBuilder Builder { get; } = new();
    
    protected override Task<DocumentSymbolResponse> Handle(DocumentSymbolParams request, CancellationToken token)
    {
        var uri = request.TextDocument.Uri.UnescapeUri;
        DocumentSymbolResponse? container = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var symbols = Builder.Build(semanticModel);
                container = new DocumentSymbolResponse(symbols);
            }
        });
        
        return Task.FromResult(container)!;
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
       serverCapabilities.DocumentSymbolProvider = new DocumentSymbolOptions()
       {
           Label = "EmmyLua"
       };
    }
}