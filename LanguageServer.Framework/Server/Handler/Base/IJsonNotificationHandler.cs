namespace EmmyLua.LanguageServer.Framework.Server.Handler.Base;

public interface IJsonNotificationHandler<in TRequest> : IJsonHandler
{
    public Task Handle(TRequest request, CancellationToken cancellationToken);
}