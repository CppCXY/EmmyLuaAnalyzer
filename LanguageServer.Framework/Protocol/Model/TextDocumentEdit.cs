using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

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
    [JsonPropertyName("edits")]
    public TextOrAnnotatedOrSnippetEditList Edits { get; init; } = null!;
}
