using EmmyLua.LanguageServer.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace EmmyLua.LanguageServer.DocumentHighlight;

// TODO
public class DocumentHighlight(ServerContext context) : DocumentHighlightHandlerBase
{
    protected override DocumentHighlightRegistrationOptions CreateRegistrationOptions(DocumentHighlightCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new()
        {
        };
    }

    public override Task<DocumentHighlightContainer?> Handle(DocumentHighlightParams request, CancellationToken cancellationToken)
    {
        return Task.FromResult<DocumentHighlightContainer?>(null);
    }
}