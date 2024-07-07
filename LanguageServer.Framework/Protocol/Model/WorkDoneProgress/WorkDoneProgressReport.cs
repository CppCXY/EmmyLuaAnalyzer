using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.WorkDoneProgress;

public record WorkDoneProgressReport() : WorkDoneProgress("report")
{
    /**
     * Controls if a cancel button should show to allow the user to cancel the long running operation.
     */
    [JsonPropertyName("cancellable")]
    public bool? Cancellable { get; init; }
    
    /**
     * Message to be displayed in the progress UI.
     */
    [JsonPropertyName("message")]
    public string? Message { get; init; }
    
    /**
     * Optional progress percentage to display. Value should be in range [0, 100].
     */
    [JsonPropertyName("percentage")]
    public double? Percentage { get; init; }
}