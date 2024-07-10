using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TypeDefinition;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public abstract class TypeDefinitionHandlerBase : IJsonHandler
{
    protected abstract Task<TypeDefinitionResponse?>
        Handle(TypeDefinitionParams request, CancellationToken cancellationToken);

    public void RegisterHandler(LanguageServer server)
    {
        server.AddRequestHandler("textDocument/typeDefinition", async (message, token) =>
        {
            var request = message.Params!.Deserialize<TypeDefinitionParams>(server.JsonSerializerOptions)!;
            var r = await Handle(request, token);
            return JsonSerializer.SerializeToDocument(r, server.JsonSerializerOptions);
        });
    }

    public abstract void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities);
}
