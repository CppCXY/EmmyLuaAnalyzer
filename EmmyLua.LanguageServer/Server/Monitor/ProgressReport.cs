using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Server.Monitor;

public class ProgressReport
{
    [JsonPropertyName("text")] public string Text { get; set; } = string.Empty;

    [JsonPropertyName("percent")] public double Percent { get; set; } = 0;
}

public class ServerStatusParams
{
    [JsonPropertyName("health")] public string Health { get; set; } = string.Empty;

    [JsonPropertyName("message")] public string? Message { get; set; } = null;

    [JsonPropertyName("loading")] public bool? Loading { get; set; } = null;
}