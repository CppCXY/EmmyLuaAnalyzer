using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace EmmyLua.LanguageServer.DocumentLink;

// ReSharper disable once ClassNeverInstantiated.Global
public class DocumentLinkHandler(ServerContext context) : DocumentLinkHandlerBase
{
    private DocumentLinkBuilder Builder { get; } = new();

    protected override DocumentLinkRegistrationOptions CreateRegistrationOptions(DocumentLinkCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new DocumentLinkRegistrationOptions
        {
            DocumentSelector = ToSelector.ToTextDocumentSelector(context.LuaWorkspace),
            ResolveProvider = false
        };
    }

    public override Task<DocumentLinkContainer?> Handle(DocumentLinkParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUri().AbsoluteUri;
        DocumentLinkContainer? container = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.LuaWorkspace.Compilation.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var document = semanticModel.Document;
                var links = Builder.Build(document, context.ResourceManager);
                container = new DocumentLinkContainer(links);
            }
        });

        return Task.FromResult(container);
    }

    public override Task<OmniSharp.Extensions.LanguageServer.Protocol.Models.DocumentLink> Handle(
        OmniSharp.Extensions.LanguageServer.Protocol.Models.DocumentLink request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}