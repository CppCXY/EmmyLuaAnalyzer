using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class DocumentSymbolClientCapabilities
{
    /**
     * Whether document symbol supports dynamic registration.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * Specific capabilities for the `SymbolKind` in the `textDocument/documentSymbol` request.
     */
    [JsonPropertyName("symbolKind")]
    public SymbolKindClientCapabilities? SymbolKind { get; init; }

    /**
     * The client support hierarchical document symbols.
     */
    [JsonPropertyName("hierarchicalDocumentSymbolSupport")]
    public bool? HierarchicalDocumentSymbolSupport { get; init; }

    /**
     * The client supports tags for individual symbol instances.
     */
    [JsonPropertyName("tagSupport")]
    public SymbolTagSupportClientCapabilities? TagSupport { get; init; }

    /**
     * The client supports an additional label presented in the UI when
     * symbol is selected.
     *
     * This label is only presented to users when the symbol is selected
     * and can be used to describe the symbol in more detail.
     */
    [JsonPropertyName("labelSupport")]
    public bool? LabelSupport { get; init; }
}

public class SymbolKindClientCapabilities
{
    /**
     * The symbol kind values the client supports. When this
     * property exists the client also guarantees that it will
     * handle values outside its set gracefully and falls back
     * to a default value when unknown.
     *
     * If this property is not present the client only supports
     * the symbol kinds from `File` to `Array` as defined in
     * the initial version of the protocol.
     */
    [JsonPropertyName("valueSet")]
    public List<SymbolKind>? ValueSet { get; init; }
}

public class SymbolTagSupportClientCapabilities
{
    /**
     * The tags supported by the client.
     */
    [JsonPropertyName("valueSet")]
    public List<SymbolTag>? ValueSet { get; init; }
}



