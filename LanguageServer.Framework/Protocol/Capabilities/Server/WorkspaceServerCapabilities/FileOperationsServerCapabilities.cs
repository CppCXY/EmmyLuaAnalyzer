using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.WorkspaceServerCapabilities;

public class FileOperationsServerCapabilities
{
    /**
     * The server is interested in receiving didCreateFiles
     * notifications.
     */
    [JsonPropertyName("didCreate")]
    public FileOperationRegistrationOptions? DidCreate { get; set; }

    /**
     * The server is interested in receiving willCreateFiles
     * requests.
     */
    [JsonPropertyName("willCreate")]
    public FileOperationRegistrationOptions? WillCreate { get; set; }

    /**
     * The server is interested in receiving didRenameFiles
     * notifications.
     */
    [JsonPropertyName("didRename")]
    public FileOperationRegistrationOptions? DidRename { get; set; }

    /**
     * The server is interested in receiving willRenameFiles
     * requests.
     */
    [JsonPropertyName("willRename")]
    public FileOperationRegistrationOptions? WillRename { get; set; }

    /**
     * The server is interested in receiving didDeleteFiles
     * notifications.
     */
    [JsonPropertyName("didDelete")]
    public FileOperationRegistrationOptions? DidDelete { get; set; }
}
