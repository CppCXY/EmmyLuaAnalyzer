using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Initialize;

public class ServerInfo
{
    /**
     * The name of the server as defined by the server.
     */
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    /**
     * The server's version string.
     */
    [JsonPropertyName("version")]
    public string? Version { get; set; }
}
