using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;

public class GeneralClientCapabilities
{
    /**
     * Client capability that signals how the client
     * handles stale requests (e.g. a request
     * for which the client will not process the response
     * anymore since the information is outdated).
     *
     * @since 3.17.0
     */
    [JsonPropertyName("staleRequestSupport")]
    public StaleRequestSupportClientCapabilities? StaleRequestSupport { get; init; }

    /**
     * Client capabilities specific to regular expressions.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("regularExpressions")]
    public RegularExpressionsClientCapabilities? RegularExpressions { get; init; }

    /**
     * Client capabilities specific to the client's markdown parser.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("markdown")]
    public MarkdownClientCapabilities? Markdown { get; init; }

    /**
     * The position encodings supported by the client. Client and server
     * have to agree on the same position encoding to ensure that offsets
     * (e.g. character position in a line) are interpreted the same on both
     * side.
     *
     * To keep the protocol backwards compatible the following applies: if
     * the value 'utf-16' is missing from the array of position encodings
     * servers can assume that the client supports UTF-16. UTF-16 is
     * therefore a mandatory encoding.
     *
     * If omitted it defaults to ['utf-16'].
     *
     * Implementation considerations: since the conversion from one encoding
     * into another requires the content of the file / line the conversion
     * is best done where the file is read which is usually on the server
     * side.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("positionEncodings")]
    public List<PositionEncodingKind>? PositionEncodings { get; init; }
}

public class StaleRequestSupportClientCapabilities
{
    /**
     * The client will actively cancel the request.
     */
    [JsonPropertyName("cancel")]
    public bool Cancel { get; init; }

    /**
     * The list of requests for which the client
     * will retry the request if it receives a
     * response with error code `ContentModified``
     */
    [JsonPropertyName("retryOnContentModified")]
    public List<string> RetryOnContentModified { get; init; } = null!;
}
