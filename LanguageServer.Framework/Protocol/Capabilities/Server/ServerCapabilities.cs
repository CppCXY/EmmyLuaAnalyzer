using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Union;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;

public class ServerCapabilities
{
    /**
     * The position encoding the server picked from the encodings offered
     * by the client via the client capability `general.positionEncodings`.
     *
     * If the client didn't provide any position encodings the only valid
     * value that a server can return is 'utf-16'.
     *
     * If omitted it defaults to 'utf-16'.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("positionEncoding")]
    public PositionEncodingKind? PositionEncoding { get; init; }

    /**
     * Defines how text documents are synced. Is either a detailed structure
     * defining each notification or for backwards compatibility the
     * TextDocumentSyncKind number. If omitted it defaults to
     * `TextDocumentSyncKind.None`.
     */
    [JsonPropertyName("textDocumentSync")]
    public TextDocumentSyncOptionsOrKind? TextDocumentSync { get; init; }

    // /**
    //  * Defines how notebook documents are synced.
    //  *
    //  * @since 3.17.0
    //  */
    // [JsonPropertyName("notebookSync")]
    // public NotebookSyncOptions? NotebookSync { get; init; }
}
