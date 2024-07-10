using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Reference;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class ReferenceHandlerBase : IJsonHandler
{
    protected abstract Task<ReferenceResponse?>
        Handle(ReferenceParams request, CancellationToken cancellationToken);

    public void RegisterHandler(LanguageServer server)
    {
        server.AddRequestHandler("textDocument/references", async (message, token) =>
        {
            var request = message.Params!.Deserialize<ReferenceParams>(server.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, server.JsonSerializerOptions);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);
}
