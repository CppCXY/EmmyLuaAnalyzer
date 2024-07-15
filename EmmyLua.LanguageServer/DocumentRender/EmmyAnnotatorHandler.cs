using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.DocumentRender;

public class EmmyAnnotatorHandler(ServerContext context) : IJsonHandler
{
    private EmmyAnnotatorBuilder Builder { get; } = new();
    
    public Task<List<EmmyAnnotatorResponse>> Handle(EmmyAnnotatorRequestParams request, CancellationToken cancellationToken)
    {
        var documentUri = request.Uri;
        var uri = documentUri.UnescapeUri;
        var response = new List<EmmyAnnotatorResponse>();
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                response = Builder.Build(semanticModel);
            }
        });

        return Task.FromResult(response);
    }

    public void RegisterHandler(Framework.Server.LanguageServer server)
    {
        server.AddRequestHandler("emmy/annotator", async (message, token) =>
        {
            var request = message.Params!.Deserialize<EmmyAnnotatorRequestParams>(server.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, server.JsonSerializerOptions);
        });
    }

    public void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
    }

    public void RegisterDynamicCapability(Framework.Server.LanguageServer server, ClientCapabilities clientCapabilities)
    {
    }
}
