using MediatR;
using Newtonsoft.Json.Linq;

namespace LanguageServer.ExecuteCommand.Commands;

public interface ICommandBase
{
    public string Name { get; }
    
    public Task<Unit> ExecuteAsync(JArray? parameters, CommandExecutor executor);
}