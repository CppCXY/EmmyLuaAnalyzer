namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Interface;

public interface IWorkDoneProgressParams
{
    /**
     * An optional token that a server can use to report work done progress.
     */
    public string? WorkDoneToken { get; set; }
}
