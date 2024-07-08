using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

public class WorkDoneProgressOptions
{
    /**
     * The server reports that it supports work done progress.
     */
    [JsonPropertyName("workDoneProgress")]
    public bool? WorkDoneProgress { get; init; }
}
