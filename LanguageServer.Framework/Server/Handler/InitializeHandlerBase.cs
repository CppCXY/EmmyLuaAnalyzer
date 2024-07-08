using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Notification;
using EmmyLua.LanguageServer.Framework.Protocol.Request.Initialize;
using EmmyLua.LanguageServer.Framework.Server.Handler.Base;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public class InitializeHandlerBase : IJsonHandler
{
    public virtual Task<InitializeResult> Handle(InitializeParams request, CancellationToken cancellationToken)
    {
        Console.Error.Write("hello world");
        var result = new InitializeResult
        {
            ServerInfo = new ServerInfo()
            {
                Name = "EmmyLua",
            },
            Capabilities = new ServerCapabilities()
        };
        return Task.FromResult(result);
    }

    public virtual Task Handle(InitializedParams request, CancellationToken cancellationToken)
    {
        Console.Error.Write("hello world2");
        return Task.CompletedTask;
    }

    public void RegisterHandler(LanguageServer server)
    {
        server.AddRequestHandler("initialize", async (message, cancelToken) =>
        {
            var request = message.Params?.Deserialize<InitializeParams>(server.JsonSerializerOptions)!;
            var r = await Handle(request, cancelToken);
            return JsonSerializer.SerializeToDocument(r, server.JsonSerializerOptions);
        });
        server.AddNotificationHandler("initialized", (message, cancelToken) =>
        {
            var request = message.Params?.Deserialize<InitializedParams>(server.JsonSerializerOptions)!;
            Handle(request, cancelToken);
        });
    }

    public void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        return;
    }
}
