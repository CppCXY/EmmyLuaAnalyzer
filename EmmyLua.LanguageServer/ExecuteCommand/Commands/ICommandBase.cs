using MediatR;
using Newtonsoft.Json.Linq;

namespace EmmyLua.LanguageServer.ExecuteCommand.Commands;

public interface ICommandBase
{
    public string Name { get; }
    
    public Task<Unit> ExecuteAsync(JArray? parameters, CommandExecutor executor);
}