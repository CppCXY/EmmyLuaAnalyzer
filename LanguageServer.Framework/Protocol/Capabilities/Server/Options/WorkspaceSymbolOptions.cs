using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

public class WorkspaceSymbolOptions : WorkDoneProgressOptions
{
    /**
     * The server provides support to resolve additional
     * information for a workspace symbol.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("resolveProvider")]
    public bool? ResolveProvider { get; set; }
}
