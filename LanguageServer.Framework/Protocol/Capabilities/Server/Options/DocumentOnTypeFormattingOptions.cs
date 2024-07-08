using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

public class DocumentOnTypeFormattingOptions
{
    /**
     * A character on which formatting should be triggered, like `{`.
     */
    [JsonPropertyName("firstTriggerCharacter")]
    public string FirstTriggerCharacter { get; set; } = null!;

    /**
     * More trigger characters.
     */
    public List<string>? MoreTriggerCharacter { get; set; }
}
