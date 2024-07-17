using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentHighlight;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;


namespace EmmyLua.LanguageServer.DocumentHighlight;

public class DocumentHighlight : DocumentHighlightHandlerBase
{
    protected override Task<DocumentHighlightResponse> Handle(DocumentHighlightParams request, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.DocumentHighlightProvider = true;
    }
}