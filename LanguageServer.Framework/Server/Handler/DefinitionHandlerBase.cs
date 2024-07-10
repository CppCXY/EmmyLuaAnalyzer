using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Definition;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class DefinitionHandlerBase : IJsonHandler
{
    protected abstract Task<DefinitionResponse?>
        Handle(DefinitionParams request, CancellationToken cancellationToken);

    public void RegisterHandler(LanguageServer server)
    {
        server.AddRequestHandler("textDocument/definition", async (message, token) =>
        {
            var request = message.Params!.Deserialize<DefinitionParams>(server.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, server.JsonSerializerOptions);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);
}
