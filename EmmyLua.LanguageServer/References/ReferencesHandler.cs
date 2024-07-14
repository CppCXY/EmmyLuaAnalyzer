using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Reference;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Util;

namespace EmmyLua.LanguageServer.References;

// ReSharper disable once ClassNeverInstantiated.Global
public class ReferencesHandler(ServerContext context) : ReferenceHandlerBase
{
    protected override Task<ReferenceResponse?> Handle(ReferenceParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.Uri.AbsoluteUri;
        ReferenceResponse? locationContainer = null;
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
                    var references = semanticModel.FindReferences(node);
                    locationContainer = new ReferenceResponse(
                        references.Select(it => it.Location.ToLspLocation()).ToList()
                    );
                }
            }
        });
        
        return Task.FromResult(locationContainer);
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.ReferencesProvider = true;
    }
}