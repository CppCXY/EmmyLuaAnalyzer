using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;

namespace EmmyLua.LanguageServer.Framework.Protocol.Server.Request.Initialize;

public class InitializeResult
{
    /**
     * The capabilities the language server provides.
     */
    [JsonPropertyName("capabilities")]
    public ServerCapabilities Capabilities { get; set; } = null!;

    /**
     * Information about the server.
     *
     * @since 3.15.0
     */
    [JsonPropertyName("serverInfo")]
    public ServerInfo? ServerInfo { get; set; }
}
