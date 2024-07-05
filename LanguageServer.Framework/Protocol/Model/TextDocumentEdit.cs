using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Util;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

public record TextDocumentEdit(
    OptionalVersionedTextDocumentIdentifier TextDocument,
    OneOf3<List<TextEdit>, List<AnnotatedTextEdit>, List<SnippetTextEdit>> Edits)
{
    /**
     * The text document to change.
     */
    public OptionalVersionedTextDocumentIdentifier TextDocument { get; } = TextDocument;

    /**
     * The edits to be applied.
     */
    [JsonPropertyName("edits"),
     JsonConverter(typeof(OneOf3JsonConverter<List<TextEdit>, List<AnnotatedTextEdit>, List<SnippetTextEdit>>))]
    public OneOf3<List<TextEdit>, List<AnnotatedTextEdit>, List<SnippetTextEdit>> Edits { get; } = Edits;
}