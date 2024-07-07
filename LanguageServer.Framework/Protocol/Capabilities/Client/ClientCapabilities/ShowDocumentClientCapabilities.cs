using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;

public class ShowDocumentClientCapabilities
{
    /**
     * The client has support for the show document
     * request.
     */
    [JsonPropertyName("support")]
    public bool? Support { get; init; }
}
