using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

// ReSharper disable once InconsistentNaming
public record Command(string Title, string? ToolTip, string Name, List<object>? Arguments)
{
    /**
     * Title of the command, like `save`.
     */
    [JsonPropertyName("title")]
    public string Title { get; } = Title;

    /**
     * The command's identifier.
     */
    [JsonPropertyName("toolTip")]
    public string? ToolTip { get; } = ToolTip;

    /**
     * The identifier of the actual command handler.
     */
    [JsonPropertyName("command")]
    // ReSharper disable once InconsistentNaming
    public string Name { get; } = Name;

    /**
     * Arguments that the command handler should be
     * invoked with.
     */
    [JsonPropertyName("arguments")]
    public List<object>? Arguments { get; } = Arguments;
}
