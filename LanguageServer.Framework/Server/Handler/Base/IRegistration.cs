using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client;

namespace EmmyLua.LanguageServer.Framework.Server.Handler.Base;

public interface IRegistration<out TOptions, in TCapability>
    where TOptions : class
    // where TCapability : ICapability
{
    TOptions GetRegistrationOptions(TCapability capability, ClientCapabilities clientCapabilities);
}