using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Registration;

/**
 * General parameters to register for a capability.
 */
public class Registration
{
    /**
     * The id used to register the request. The id can be used to deregister
     * the request again.
     */
    [JsonPropertyName("id")]
    public string Id { get; set; } = null!;

    /**
     * The method / capability to register for.
     */
    [JsonPropertyName("method")]
    public string Method { get; set; } = null!;

    /**
     * Options necessary for the registration.
     */
    [JsonPropertyName("registerOptions")]
    public JsonDocument? RegisterOptions { get; set; }
}
