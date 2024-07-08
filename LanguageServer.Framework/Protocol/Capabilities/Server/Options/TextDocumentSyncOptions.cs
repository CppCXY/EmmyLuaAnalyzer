using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

public class TextDocumentSyncOptions
{
    /**
     * Open and close notifications are sent to the server. If omitted open
     * close notification should not be sent.
     */
    [JsonPropertyName("openClose")]
    public bool OpenClose { get; init; }

    /**
     * Change notifications are sent to the server. See
     * TextDocumentSyncKind.None, TextDocumentSyncKind.Full and
     * TextDocumentSyncKind.Incremental. If omitted it defaults to
     * TextDocumentSyncKind.None.
     */
    [JsonPropertyName("change")]
    public TextDocumentSyncKind? Change { get; init; }

}
