using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Implementation;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Util;

namespace EmmyLua.LanguageServer.Implementation;

public class ImplementationHandler(ServerContext context) : ImplementationHandlerBase
{
    protected override Task<ImplementationResponse?> Handle(ImplementationParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.UnescapeUri;
        ImplementationResponse? locationContainer = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var document = semanticModel.Document;
                var pos = request.Position;
                var node = document.SyntaxTree.SyntaxRoot.NameNodeAt(pos.Line, pos.Character);
                if (node is not null)
                {
                    var implementations = semanticModel.FindImplementations(node);
                    locationContainer = new (
                        implementations.Select(it => it.Location.ToLspLocation()).ToList()
                    );
                }
            }
        });
        
        return Task.FromResult(locationContainer);
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.ImplementationProvider = true;
    }
}