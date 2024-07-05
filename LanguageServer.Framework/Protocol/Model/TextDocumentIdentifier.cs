using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

public record TextDocumentIdentifier(DocumentUri Uri)
{
    /**
     * The text document's URI.
     */
    [JsonPropertyName("uri")]
    public DocumentUri Uri { get; } = Uri;
}

public record VersionedTextDocumentIdentifier(DocumentUri Uri, int Version)
    : TextDocumentIdentifier(Uri)
{
    /**
     * The version number of this document.
     *
     * The version number of a document will increase after each change,
     * including undo/redo. The number doesn't need to be consecutive.
     */
    [JsonPropertyName("version")]
    public int Version { get; } = Version;
}

public record OptionalVersionedTextDocumentIdentifier(DocumentUri Uri, int? Version)
    : TextDocumentIdentifier(Uri)
{
    /**
     * The version number of this document. If an optional versioned text document
     * identifier is sent from the server to the client and the file is not
     * open in the editor (the server has not received an open notification
     * before) the server can send `null` to indicate that the version is
     * known and the content on disk is the master (as specified with document
     * content ownership).
     *
     * The version number of a document will increase after each change,
     * including undo/redo. The number doesn't need to be consecutive.
     */
    [JsonPropertyName("version")]
    public int? Version { get; } = Version;
}