using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Protocol.Server.Request.Initialize;
using EmmyLua.LanguageServer.Framework.Server.Handler.Base;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public class InitializeHandlerBase : IJsonRpcRequestHandler<InitializeParams, InitializeResponse>
{
    [JsonRpc("initialize")]
    public virtual Task<InitializeResponse> Handle(InitializeParams request, CancellationToken cancellationToken)
    {
        Console.Error.Write("hello world");
        return Task.FromResult(new InitializeResponse());
    }
}