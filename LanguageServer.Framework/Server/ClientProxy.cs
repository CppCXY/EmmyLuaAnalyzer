using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Registration;

namespace EmmyLua.LanguageServer.Framework.Server;

public class ClientProxy(LanguageServer server)
{
    public Task DynamicRegisterCapability(RegistrationParams @params)
    {
        var document = JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions);
        var notification = new NotificationMessage("client/registerCapability", document);
        return server.SendNotification(notification);
    }

    public Task DynamicUnregisterCapability(UnregistrationParams @params)
    {
        var document = JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions);
        var notification = new NotificationMessage("client/unregisterCapability", document);
        return server.SendNotification(notification);
    }
}
