using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentLink;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.DocumentLink;

// ReSharper disable once ClassNeverInstantiated.Global
public class DocumentLinkHandler(ServerContext context) : DocumentLinkHandlerBase
{
    private DocumentLinkBuilder Builder { get; } = new(context);
    
    protected override Task<DocumentLinkResponse> Handle(DocumentLinkParams request, CancellationToken token)
    {
        var uri = request.TextDocument.Uri.UnescapeUri;
        DocumentLinkResponse? container = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.LuaWorkspace.Compilation.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var document = semanticModel.Document;
                var links = Builder.Build(document, context.ResourceManager);
                container = new DocumentLinkResponse(links);
            }
        });
        
        return Task.FromResult(container)!;
    }

    protected override Task<Framework.Protocol.Message.DocumentLink.DocumentLink> Resolve(Framework.Protocol.Message.DocumentLink.DocumentLink request, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.DocumentLinkProvider = new DocumentLinkOptions()
        {
            ResolveProvider = false
        };
    }
}