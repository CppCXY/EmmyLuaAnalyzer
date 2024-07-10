using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Notebook;

public class ExecutionSummary
{
    /**
     * A strictly monotonically increasing value
     * indicating the execution order of a cell
     * inside a notebook.
     */
    [JsonPropertyName("executedOrder")]
    public uint ExecutedOrder { get; set; }

    /**
     * Whether the execution was successful or
     * not if known by the client.
     */
    [JsonPropertyName("success")]
    public bool? Success { get; set; }
}
