using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Util;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

public record TextDocumentEdit
{
    /**
     * The text document to change.
     */
    public OptionalVersionedTextDocumentIdentifier TextDocument { get; init; } = null!;

    /**
     * The edits to be applied.
     */
    [JsonPropertyName("edits"),
     JsonConverter(typeof(OneOf3JsonConverter<List<TextEdit>, List<AnnotatedTextEdit>, List<SnippetTextEdit>>))]
    public OneOf<List<TextEdit>, List<AnnotatedTextEdit>, List<SnippetTextEdit>> Edits { get; init; } = null!;
}