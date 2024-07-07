using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class FoldingRangeClientCapabilities
{
    /**
     * Whether implementation supports dynamic registration. If this is set to `true`
     * the client supports the new `ImplementationRegistrationOptions` return value
     * for the corresponding server capability as well.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * The client supports folding range requests.
     */
    [JsonPropertyName("rangeLimit")]
    public int? RangeLimit { get; init; }

    /**
     * If set, the client will ignore requests to provide folding ranges
     * and instead rely on the range information from the text document.
     */
    [JsonPropertyName("lineFoldingOnly")]
    public bool? LineFoldingOnly { get; init; }

    /**
     * Specific options for the folding range kind.
     *
     * @since 3.17.0
     */
    [JsonPropertyName("foldingRangeKind")]
    public FoldingRangeKindClientCapabilities? FoldingRangeKind { get; init; }

    /**
     * Specific options for the folding range.
     * @since 3.17.0
     */
    [JsonPropertyName("foldingRange")]
    public FoldingRangeStructClientCapabilities? FoldingRange { get; init; }
}

public class FoldingRangeKindClientCapabilities
{
    /**
     * The folding range kind values the client supports. When this
     * property exists the client also guarantees that it will
     * handle values outside its set gracefully and falls back
     * to a default value when unknown.
     */
    [JsonPropertyName("valueSet")]
    public List<FoldingRangeKind>? ValueSet { get; init; }
}

public class FoldingRangeStructClientCapabilities
{
    /**
    * If set, the client signals that it supports setting collapsedText on
    * folding ranges to display custom labels instead of the default text.
    *
    * @since 3.17.0
    */
    [JsonPropertyName("collapsedText")]
    public bool? CollapsedText { get; init; }
}
