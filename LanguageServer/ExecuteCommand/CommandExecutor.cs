using LanguageServer.ExecuteCommand.Commands;
using LanguageServer.Server;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace LanguageServer.ExecuteCommand;

public class CommandExecutor(
    ServerContext context,
    ILogger<ExecuteCommandHandler> logger)
{
    private ILanguageServerFacade Facade { get; } = context.Server;

    public ServerContext Context { get; } = context;
    
    private List<ICommandBase> Commands { get; } =
    [
        new AutoRequire(),
        new DiagnosticAction(),
        new SetConfig()
    ];

    public List<string> GetCommands()
    {
        return Commands.Select(c => c.Name).ToList();
    }

    public async Task<Unit> ExecuteAsync(string command, JArray? arguments)
    {
        var cmd = Commands.FirstOrDefault(c => c.Name == command);
        if (cmd is not null)
        {
            await cmd.ExecuteAsync(arguments, this);
        }
        
        return await Unit.Task;
    }

    public async Task<Unit> ApplyEditAsync(string uri, TextEdit textEdit)
    {
        var response = await Facade.Workspace.ApplyWorkspaceEdit(new ApplyWorkspaceEditParams()
        {
            Edit = new WorkspaceEdit()
            {
                Changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>()
                {
                    {
                        uri, new TextEditContainer(textEdit)
                    }
                }
            }
        });

        if (!response.Applied)
        {
            logger.LogError(response.FailureReason);
        }

        return await Unit.Task;
    }
}