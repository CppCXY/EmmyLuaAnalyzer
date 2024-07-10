using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Implementation;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class ImplementationHandlerBase : IJsonHandler
{
    protected abstract Task<ImplementationResponse?>
        Handle(ImplementationParams request, CancellationToken cancellationToken);

    public void RegisterHandler(LanguageServer server)
    {
        server.AddRequestHandler("textDocument/implementation", async (message, token) =>
        {
            var request = message.Params!.Deserialize<ImplementationParams>(server.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, server.JsonSerializerOptions);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);
}
