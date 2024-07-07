using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class RenameClientCapabilities
{
    /**
     * Whether rename supports dynamic registration.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * Client supports testing for validity of rename operations
     * before execution.
     *
     * @since version 3.12.0
     */
    [JsonPropertyName("prepareSupport")]
    public bool? PrepareSupport { get; init; }

    /**
     * Client supports the default behavior result (`{ defaultBehavior: boolean }`).
     *
     * @since version 3.16.0
     */
    [JsonPropertyName("prepareSupportDefaultBehavior")]
    public PrepareSupportDefaultBehavior? PrepareSupportDefaultBehavior { get; init; }

    /**
     * Whether the client honors the change annotations in
     * text edits and resource operations returned via the
     * rename request's workspace edit by, for example, presenting
     * the workspace edit in the user interface and asking
     * for confirmation.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("honorsChangeAnnotations")]
    public bool? HonorsChangeAnnotations { get; init; }
}


