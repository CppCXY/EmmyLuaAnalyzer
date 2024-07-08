using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

public class SignatureHelpOptions : WorkDoneProgressOptions
{
    /**
     * The characters that trigger signature help
     * automatically.
     */
    [JsonPropertyName("triggerCharacters")]
    public List<string>? TriggerCharacters { get; init; }

    /**
     * List of characters that re-trigger signature help.
     *
     * These trigger characters are only active when signature help is already
     * showing. All trigger characters are also counted as re-trigger
     * characters.
     *
     * @since 3.15.0
     */
    [JsonPropertyName("retriggerCharacters")]
    public List<string>? RetriggerCharacters { get; init; }
}
