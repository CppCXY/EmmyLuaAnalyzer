using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.TextDocumentClientCapabilities;

public class TextDocumentSyncClientCapabilities
{
    /**
     * Whether text document synchronization supports dynamic registration.
     */
    [JsonPropertyName("dynamicRegistration")]
    public bool? DynamicRegistration { get; init; }

    /**
     * The client supports sending will save notifications.
     */
    [JsonPropertyName("willSave")]
    public bool? WillSave { get; init; }

    /**
     * The client supports sending a will save request and
     * waits for a response providing text edits which will
     * be applied to the document before it is saved.
     */
    [JsonPropertyName("willSaveWaitUntil")]
    public bool? WillSaveWaitUntil { get; init; }

    /**
     * The client supports did save notifications.
     */
    [JsonPropertyName("didSave")]
    public bool? DidSave { get; init; }
}