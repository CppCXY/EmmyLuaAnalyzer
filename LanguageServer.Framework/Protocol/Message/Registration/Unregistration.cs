using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Registration;

/**
 * General parameters to unregister a capability.
 */
public class Unregistration
{
    /**
     * The id used to unregister the request or notification. Usually an id
     * provided during the register request.
     */
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    /**
     * The method / capability to unregister for.
     */
    [JsonPropertyName("method")]
    public string Method { get; set; } = null!;
}
