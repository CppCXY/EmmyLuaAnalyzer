using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Initialize;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

internal class InitializeHandler(LanguageServer server) : IJsonHandler
{
    private Task<InitializeResult> Handle(InitializeParams request, CancellationToken cancellationToken)
    {
        var serverInfo = new ServerInfo();
        var capabilities = new ServerCapabilities();
        foreach (var handler in server.Handlers)
        {
            handler.RegisterCapability(capabilities, request.Capabilities);
        }

        server.InitializeEventDelegate?.Invoke(request, serverInfo);
        var result = new InitializeResult
        {
            ServerInfo = serverInfo,
            Capabilities = capabilities
        };
        return Task.FromResult(result);
    }

    private Task Handle(InitializedParams request, CancellationToken cancellationToken)
    {
        server.InitializedEventDelegate?.Invoke(request);
        return Task.CompletedTask;
    }

    public void RegisterHandler(LanguageServer server2)
    {
        server2.AddRequestHandler("initialize", async (message, cancelToken) =>
        {
            var request = message.Params?.Deserialize<InitializeParams>(server2.JsonSerializerOptions)!;
            var r = await Handle(request, cancelToken);
            return JsonSerializer.SerializeToDocument(r, server2.JsonSerializerOptions);
        });
        server2.AddNotificationHandler("initialized", (message, cancelToken) =>
        {
            var request = message.Params?.Deserialize<InitializedParams>(server2.JsonSerializerOptions)!;
            return Handle(request, cancelToken);
        });
    }

    public void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        return;
    }
}
