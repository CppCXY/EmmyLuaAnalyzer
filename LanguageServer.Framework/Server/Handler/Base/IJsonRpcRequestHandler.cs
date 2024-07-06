namespace EmmyLua.LanguageServer.Framework.Server.Handler.Base;

public interface IJsonRpcRequestHandler<in TRequest, TResponse> : IJsonHandler
{
    public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}