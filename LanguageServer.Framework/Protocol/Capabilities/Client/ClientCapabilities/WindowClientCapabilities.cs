using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;

public class WindowClientCapabilities
{
    /**
     * It indicates whether the client supports server initiated
     * progress using the `window/workDoneProgress/create` request.
     *
     * The capability also controls Whether client supports handling
     * of progress notifications. If set servers are allowed to report a
     * `workDoneProgress` property in the request specific server
     * capabilities.
     *
     * @since 3.15.0
     */
    [JsonPropertyName("workDoneProgress")]
    public bool? WorkDoneProgress { get; init; }

    /**
     * Capabilities specific to the showMessage request
     *
     * @since 3.16.0
     */
    [JsonPropertyName("showMessage")]
    public ShowMessageRequestClientCapabilities? ShowMessage { get; init; }

    /**
     * Client capabilities for the show document request.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("showDocument")]
    public ShowDocumentClientCapabilities? ShowDocument { get; init; }


}
