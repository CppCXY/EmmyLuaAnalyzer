using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.WorkDoneProgress;

public record WorkDoneProgressEnd() : WorkDoneProgress("end")
{
    /**
     * Optional, final message indicating to for example indicate the outcome of the operation.
     */
    [JsonPropertyName("message")]
    public string? Message { get; init; }
}