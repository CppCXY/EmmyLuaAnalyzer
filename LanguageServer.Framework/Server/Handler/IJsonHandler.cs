using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;

namespace EmmyLua.LanguageServer.Framework.Server.Handler;

public interface IJsonHandler
{
    public void RegisterHandler(LanguageServer server);

    public void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities);
}
