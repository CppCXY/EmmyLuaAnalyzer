using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.WorkspaceServerCapabilities;

public class WorkspaceFoldersServerCapabilities
{
    /**
     * The server has support for workspace folders.
     */
    [JsonPropertyName("supported")]
    public bool? Supported { get; set; }

    /**
     * Whether the server wants to receive workspace folder
     * change notifications.
     *
     * If a string is provided, the string is treated as an ID
     * under which the notification is registered on the client
     * side. The ID can be used to unregister for these events
     * using the `client/unregisterCapability` request.
     */
    [JsonPropertyName("changeNotifications")]
    public BooleanOr<string>? ChangeNotifications { get; set; }
}
