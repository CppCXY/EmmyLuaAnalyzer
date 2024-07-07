using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.WorkspaceEditClientCapabilities;

public record struct ChangeAnnotationSupport
{
    /**
     * Whether the client groups edits with equal labels into tree nodes,
     * for instance all edits labelled with "Changes in Strings" would
     * be a tree node.
     */
    [JsonPropertyName("groupsOnLabel")]
    public bool? GroupsOnLabel { get; init; }
}