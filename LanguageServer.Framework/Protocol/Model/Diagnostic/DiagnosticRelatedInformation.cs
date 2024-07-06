using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;

[method: JsonConstructor]
public record struct DiagnosticRelatedInformation(Location Location, string Message)
{
    /**
     * The location of this related diagnostic information.
     */
    [JsonPropertyName("location")]
    public Location Location { get; } = Location;

    /**
     * The message of this related diagnostic information.
     */
    [JsonPropertyName("message")]
    public string Message { get; } = Message;
}