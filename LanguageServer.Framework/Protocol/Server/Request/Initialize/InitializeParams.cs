using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Server;

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
    
}