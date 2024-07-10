using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Notebook;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.NotebookDocument;

/**
 * The params sent in a save notebook document notification.
 *
 * @since 3.17.0
 */
public class DidSaveNotebookDocumentParams
{
    /**
     * The notebook document that got saved.
     */
    [JsonPropertyName("notebookDocument")]
    public NotebookDocumentIdentifier NotebookDocument { get; set; } = null!;
}
