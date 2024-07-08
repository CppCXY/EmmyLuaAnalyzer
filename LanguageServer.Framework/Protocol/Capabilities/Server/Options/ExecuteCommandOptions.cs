using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

public class ExecuteCommandOptions : WorkDoneProgressOptions
{
    /**
     * The commands to be executed on the server.
     */
    [JsonPropertyName("commands")]
    public List<string> Commands { get; init; } = [];
}
