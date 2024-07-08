using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.WorkspaceServerCapabilities;

public class WorkspaceServerCapabilities
{
    /**
     * The server supports workspace folder.
     *
     * @since 3.6.0
     */
    [JsonPropertyName("workspaceFolders")]
    public WorkspaceFoldersServerCapabilities? WorkspaceFolders { get; set; }

    /**
     * The server is interested in file notifications/requests.
     *
     * @since 3.16.0
     */
    [JsonPropertyName("fileOperations")]
    public FileOperationsServerCapabilities? FileOperations { get; set; }
}
