using System.Text.Json;
using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Server.Handler.Base;
using EmmyLua.LanguageServer.Framework.Server.JsonProtocol;
using EmmyLua.LanguageServer.Framework.Server.Scheduler;

namespace EmmyLua.LanguageServer.Framework.Server;

public class LanguageServer
{
    public JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        TypeInfoResolver = JsonProtocolContext.Default,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private JsonProtocolReader Reader { get; }

    private JsonProtocolWriter Writer { get; }

    private bool IsRunning { get; set; }

    private IScheduler Scheduler { get; set; } = new SingleThreadScheduler();

    private List<IJsonHandler> Handlers { get; } = new();

    private Dictionary<string, Func<RequestMessage, CancellationToken,Task<JsonDocument>>> RequestHandlers { get; } = new();

    private Dictionary<string, Action<NotificationMessage, CancellationToken>> NotificationHandlers { get; } = new();

    private LanguageServer(Stream input, Stream output)
    {
        Reader = new JsonProtocolReader(input, JsonSerializerOptions);
        Writer = new JsonProtocolWriter(output, JsonSerializerOptions);
    }

    public static LanguageServer From(Stream input, Stream output)
    {
        return new LanguageServer(input, output);
    }

    public LanguageServer SupportMultiThread()
    {
        Scheduler = new MultiThreadScheduler();
        return this;
    }

    public void AddJsonSerializeContext(JsonSerializerContext serializerContext)
    {
        JsonSerializerOptions.TypeInfoResolverChain.Add(serializerContext);
    }

    public void AddRequestHandler(string method, Func<RequestMessage, CancellationToken, Task<JsonDocument>> handler)
    {
        RequestHandlers[method] = handler;
    }

    public void AddNotificationHandler(string method, Action<NotificationMessage, CancellationToken> handler)
    {
        NotificationHandlers[method] = handler;
    }

    public LanguageServer AddHandler(IJsonHandler handler)
    {
        Handlers.Add(handler);
        handler.RegisterHandler(this);
        return this;
    }

    public Task SendNotification(NotificationMessage notification)
    {
        Writer.WriteNotification(notification);
        return Task.CompletedTask;
    }

    public async Task Run()
    {
        try
        {
            while (!IsRunning)
            {
                var message = await Reader.ReadAsync();
                Scheduler.Schedule(async () =>
                {
                    switch (message)
                    {
                        case RequestMessage request:
                        {
                            if (RequestHandlers.TryGetValue(request.Method, out var handler))
                            {
                                var result = await handler(request, CancellationToken.None).ConfigureAwait(false);
                                Writer.WriteResponse(request.Id, result);
                            }

                            break;
                        }
                        case NotificationMessage notification:
                        {
                            if (notification.Method == "shutdown")
                            {
                                IsRunning = true;
                            }

                            if (NotificationHandlers.TryGetValue(notification.Method, out var handler))
                            {
                                handler(notification, CancellationToken.None);
                            }

                            break;
                        }
                    }
                });
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }
}
