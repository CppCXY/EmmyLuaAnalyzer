using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Protocol.Notification;
using EmmyLua.LanguageServer.Framework.Protocol.Request.Initialize;
using EmmyLua.LanguageServer.Framework.Server.Handler.Base;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public class InitializeHandlerBase : IJsonRpcRequestHandler<InitializeParams, InitializeResult>
{
    [JsonRpc("initialize")]
    public virtual Task<InitializeResult> Handle(InitializeParams request, CancellationToken cancellationToken)
    {
        Console.Error.Write("hello world");
        return Task.FromResult(new InitializeResult());
    }

    [JsonRpc("initialized")]
    public virtual Task Handle(InitializedParams request, CancellationToken cancellationToken)
    {
        Console.Error.Write("hello world2");
        return Task.CompletedTask;
    }
}
