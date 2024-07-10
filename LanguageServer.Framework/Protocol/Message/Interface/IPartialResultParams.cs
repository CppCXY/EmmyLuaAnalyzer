namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Interface;

public interface IPartialResultParams
{
    /**
     * An optional token that a server can use to report partial results (e.g.
     * streaming) to the client.
     */
    public string? PartialResultToken { get; set; }
}
