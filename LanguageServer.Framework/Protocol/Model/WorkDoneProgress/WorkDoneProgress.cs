using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.WorkDoneProgress;

public record WorkDoneProgress(string Kind)
{
    [JsonPropertyName("kind")]
    public string Kind { get; init; } = Kind;
}

