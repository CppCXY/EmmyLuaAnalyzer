using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using EmmyLua.LanguageServer.Framework.Protocol;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Server.Handler.Base;
using EmmyLua.LanguageServer.Framework.Server.JsonProtocol;
using EmmyLua.LanguageServer.Framework.Server.Scheduler;
using Microsoft.Extensions.DependencyInjection;

namespace EmmyLua.LanguageServer.Framework.Server;

public class LanguageServer
{
    private JsonSerializerOptions JsonSerializerOptions { get; } = new()
    {
        TypeInfoResolver = JsonProtocolContext.Default
    };
    
    private JsonProtocolReader Reader { get; }

    private JsonProtocolWriter Writer { get; }
    
    private bool IsInitialized { get; set; }

    private bool IsRunning { get; set; }

    private ServiceCollection Services { get; } = new();

    private ServiceProvider? ServiceProvider { get; set; }

    private IScheduler Scheduler { get; set; } = new SingleThreadScheduler();

    record struct JsonRpcRequestHandler(MethodInfo Method, Type ReturnType, IJsonHandler Instance);

    private Dictionary<string, JsonRpcRequestHandler> RequestHandlers { get; } = new();

    record struct JsonRpcNotificationandler(MethodInfo Method, IJsonHandler Instance);

    private Dictionary<string, JsonRpcNotificationandler> NotificationHandlers { get; } = new();

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

    public LanguageServer WithHandler<THandler>() where THandler : class, IJsonHandler
    {
        Services.AddSingleton<THandler>();
        return this;
    }

    public LanguageServer WithServices(Action<IServiceCollection> servicesAction)
    {
        servicesAction(Services);
        return this;
    }

    public async Task SendNotification(NotificationMessage notification)
    {
        await Writer.WriteAsync(notification, typeof(NotificationMessage));
    }

    private void InitServices()
    {
        ServiceProvider = Services.BuildServiceProvider();

        foreach (var serviceDescriptor in Services)
        {
            if (serviceDescriptor.ImplementationType is null)
            {
                continue;
            }

            var service = ServiceProvider.GetService(serviceDescriptor.ImplementationType);
            if (service is IJsonHandler handler)
            {
                foreach (var method in handler.GetType().GetMethods())
                {
                    if (method.GetCustomAttribute<JsonRpcAttribute>() is not { } attribute)
                    {
                        continue;
                    }

                    // 检查handler是否实现了IJsonRpcRequestHandler接口
                    var handlerInterfaces = serviceDescriptor.ImplementationType?.GetInterfaces();
                    var isRequestHandler = handlerInterfaces?.Any(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IJsonRpcRequestHandler<,>));

                    if (isRequestHandler is true)
                    {
                        var responseType = handlerInterfaces?.First(i =>
                                i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IJsonRpcRequestHandler<,>))
                            .GetGenericArguments()[1]!;

                        RequestHandlers[attribute.Method] = new JsonRpcRequestHandler(method, responseType, handler);
                    }
                    else
                    {
                        NotificationHandlers[attribute.Method] = new JsonRpcNotificationandler(method, handler);
                    }
                }
            }
        }
    }

    public async Task Run()
    {
        InitServices();
        try
        {
            while (!IsRunning)
            {
                var message = await Reader.ReadAsync();
                switch (message)
                {
                    case RequestMessage request:
                    {
                        if (RequestHandlers.TryGetValue(request.Method, out var handler))
                        {
                            var result = handler.Method.Invoke(handler.Instance,
                                [request.Params, CancellationToken.None]);
                            if (result is Task task)
                            {
                                await task.ConfigureAwait(false);
                                if (task.GetType().IsGenericType)
                                {
                                    result = task.GetType().GetProperty("Result")?.GetValue(task);
                                    await Writer.WriteAsync(result, handler.ReturnType);
                                }
                                else
                                {
                                    await Writer.WriteAsync(result, handler.ReturnType);
                                }
                            }
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
                            var result = handler.Method.Invoke(handler.Instance, [notification.Params]);
                            if (result is Task task)
                            {
                                await task.ConfigureAwait(false);
                            }
                        }

                        break;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }
}