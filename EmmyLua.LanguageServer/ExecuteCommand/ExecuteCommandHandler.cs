using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.ExecuteCommand;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.ExecuteCommand;

// ReSharper disable once ClassNeverInstantiated.Global
public class ExecuteCommandHandler(ServerContext context) : ExecuteCommandHandlerBase
{
    private CommandExecutor Executor { get; } = new(context);
    
    protected override Task<ExecuteCommandResponse> Handle(ExecuteCommandParams request, CancellationToken token)
    {
        Executor.ExecuteAsync(request.Command, request.Arguments).WaitAsync(token);
        return Task.FromResult(new ExecuteCommandResponse(null));
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.ExecuteCommandProvider = new ExecuteCommandOptions()
        {
            Commands = Executor.GetCommands()
        };
    }
}