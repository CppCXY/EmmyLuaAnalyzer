using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.DocumentHighlight;

// TODO
public class DocumentHighlight(LuaWorkspace workspace) : DocumentHighlightHandlerBase
{
    protected override DocumentHighlightRegistrationOptions CreateRegistrationOptions(DocumentHighlightCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new()
        {
            DocumentSelector = ToSelector.ToTextDocumentSelector(workspace),
        };
    }

    public override Task<DocumentHighlightContainer?> Handle(DocumentHighlightParams request, CancellationToken cancellationToken)
    {
        return Task.FromResult<DocumentHighlightContainer?>(null);
    }
}