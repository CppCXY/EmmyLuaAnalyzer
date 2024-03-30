using EmmyLua.CodeAnalysis.Workspace;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace LanguageServer.ExecuteCommand;

// ReSharper disable once ClassNeverInstantiated.Global
public class ExecuteCommandHandler(
    LuaWorkspace workspace,
    ILanguageServerFacade languageServerFacade,
    ILogger<ExecuteCommandHandler> logger) : ExecuteCommandHandlerBase
{
    private CommandExecutor Executor { get; } = new(workspace, languageServerFacade, logger);

    protected override ExecuteCommandRegistrationOptions CreateRegistrationOptions(ExecuteCommandCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new ExecuteCommandRegistrationOptions()
        {
            Commands = Executor.GetCommands()
        };
    }

    public override Task<Unit> Handle(ExecuteCommandParams request, CancellationToken cancellationToken)
    {
        return Executor.ExecuteAsync(request.Command, request.Arguments);
    }
}