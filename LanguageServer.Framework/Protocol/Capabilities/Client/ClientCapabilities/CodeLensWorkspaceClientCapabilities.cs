using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;

public class CodeLensWorkspaceClientCapabilities
{
    /**
     * Whether the client implementation supports a refresh request sent from the
     * server to the client.
     *
     * Note that this event is global and will force the client to refresh all
     * code lenses currently shown. It should be used with absolute care and is
     * useful for situation where a server, for example, detects a project wide
     * change that requires such a calculation.
     */
    [JsonPropertyName("refreshSupport")]
    public bool? RefreshSupport { get; init; }
}
