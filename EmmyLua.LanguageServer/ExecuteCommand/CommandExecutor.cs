using EmmyLua.LanguageServer.ExecuteCommand.Commands;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.ApplyWorkspaceEdit;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.ExecuteCommand;

public class CommandExecutor(ServerContext context)
{
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

    public async Task ExecuteAsync(string command, List<LSPAny>? arguments)
    {
        var cmd = Commands.FirstOrDefault(c => c.Name == command);
        if (cmd is not null)
        {
            await cmd.ExecuteAsync(arguments, this);
        }
    }

    public async Task ApplyEditAsync(string uri, TextEdit textEdit)
    {
        var response = await Context.Server.Client.ApplyEdit(new ApplyWorkspaceEditParams()
        {
            Edit = new WorkspaceEdit()
            {
                Changes =  new ()
                {
                    { new DocumentUri(new Uri(uri)), new List<TextEdit> { textEdit } }
                }
            }
        }, CancellationToken.None);

        if (!response.Applied)
        {
            await Console.Error.WriteLineAsync(response.FailureReason);
        }
    }
}