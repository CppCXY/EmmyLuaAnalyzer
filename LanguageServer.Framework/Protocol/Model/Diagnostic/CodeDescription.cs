using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;

[method: JsonConstructor]
public readonly record struct CodeDescription(Uri Href)
{
    /**
     * An URI to open with more information about the diagnostic error.
     */
    [JsonPropertyName("href")]
    public Uri Href { get; } = Href;
}