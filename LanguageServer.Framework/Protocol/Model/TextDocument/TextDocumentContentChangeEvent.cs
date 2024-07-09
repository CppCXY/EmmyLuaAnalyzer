using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;

public class TextDocumentContentChangeEvent
{
    /**
     * The range of the document that changed.
     */
    [JsonPropertyName("range")]
    public Range? Range { get; set; }

    /**
     * The length of the range that got replaced.
     */
    [JsonPropertyName("rangeLength")]
    public int? RangeLength { get; set; }

    /**
     * The new text of the range/document.
     */
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
