using Newtonsoft.Json;

namespace LanguageServer.Server.Monitor;

public class ProgressReport
{
    [JsonProperty("text")] public string Text { get; set; } = string.Empty;

    [JsonProperty("percent")] public double Percent { get; set; } = 0;
}

public class ServerStatusParams
{
    [JsonProperty("health")] public string Health { get; set; } = string.Empty;

    [JsonProperty("message")] public string? Message { get; set; } = null;

    [JsonProperty("loading")] public bool? Loading { get; set; } = null;
}