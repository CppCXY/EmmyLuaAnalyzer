using System.Text.Json;
using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;

public class ClientCapabilities
{
    /**
     * Workspace specific client capabilities.
     */
    [JsonPropertyName("workspace")]
    public WorkspaceClientCapabilities? Workspace { get; init; }

    /**
     * Text document specific client capabilities.
     */
    [JsonPropertyName("textDocument")]
    public TextDocumentClientCapabilities.TextDocumentClientCapabilities? TextDocument { get; init; }

    /**
     * Capabilities specific to the notebook document support.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("notebookDocument")]
    public NotebookDocumentClientCapabilities.NotebookDocumentClientCapabilities? NotebookDocument { get; init; }

    /**
     * Window specific client capabilities.
     */
    [JsonPropertyName("window")]
    public WindowClientCapabilities? Window { get; init; }

    /**
     * General client capabilities.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("general")]
    public GeneralClientCapabilities? General { get; init; }


    /**
     * Experimental client capabilities.
     */
    [JsonPropertyName("experimental")]
    public JsonDocument? Experimental { get; init; }
}
