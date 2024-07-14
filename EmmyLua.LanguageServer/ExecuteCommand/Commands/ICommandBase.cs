using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace EmmyLua.LanguageServer.ExecuteCommand.Commands;

public interface ICommandBase
{
    public string Name { get; }
    
    public Task ExecuteAsync(List<LSPAny>? parameters, CommandExecutor executor);
}