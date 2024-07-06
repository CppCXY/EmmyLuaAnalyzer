using System.Text.Json;
using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace EmmyLua.LanguageServer.Framework.Protocol.Server.Request.Initialize;

[JsonRpc("initialize")]
public class InitializeParams
{
    /**
    * The process Id of the parent process that started the server. Is null if
    * the process has not been started by another process. If the parent
    * process is not alive then the server should exit (see exit notification)
    * its process.
    */
    [JsonPropertyName("processId")]
    public int? ProcessId { get; set; }
    
    /**
     * Information about the client
     *
     * @since 3.15.0
     */
    [JsonPropertyName("clientInfo")]
    public ClientInfo? ClientInfo { get; set; }
    
    /**
     * The locale the client is currently showing the user interface
     * in. This must not necessarily be the locale of the operating
     * system.
     *
     * Uses IETF language tags as the value's syntax
     * (See https://en.wikipedia.org/wiki/IETF_language_tag)
     *
     * @since 3.16.0
     */
    [JsonPropertyName("locale")]
    public string? Locale { get; set; }
    
    /**
     * The rootPath of the workspace. Is null
     * if no folder is open.
     *
     * @deprecated in favour of rootUri.
     */
    [JsonPropertyName("rootPath"), Obsolete("Use `rootUri` instead.")]
    public string? RootPath { get; set; }
    
    /**
     * The rootUri of the workspace. Is null if no
     * folder is open. If both `rootPath` and `rootUri` are set
     * `rootUri` wins.
     */
    [JsonPropertyName("rootUri")]
    public DocumentUri? RootUri { get; set; }
    
    /**
     * User provided initialization options.
     */
    [JsonPropertyName("initializationOptions")]
    public JsonDocument? InitializationOptions { get; set; }

    /**
     * The capabilities provided by the client (editor or tool)
     */
    [JsonPropertyName("capabilities")]
    public ClientCapabilities Capabilities { get; set; } = new ClientCapabilities();

    /**
     * The initial trace setting. If omitted trace is disabled ('off').
     */
    [JsonPropertyName("trace")]
    public TraceValue? Trace { get; set; }
    
    /**
     * The workspace folders configured in the client when the server starts.
     * This property is only available if the client supports workspace folders.
     * It can be `null` if the client supports workspace folders but none are
     * configured.
     *
     * @since 3.6.0
     */
    [JsonPropertyName("workspaceFolders")]
    public List<WorkspaceFolder>? WorkspaceFolders { get; set; }
}