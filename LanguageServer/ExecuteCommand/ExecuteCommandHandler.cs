using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.Server;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace LanguageServer.ExecuteCommand;

// ReSharper disable once ClassNeverInstantiated.Global
public class ExecuteCommandHandler(
    ServerContext context,
    ILogger<ExecuteCommandHandler> logger) : ExecuteCommandHandlerBase
{
    private CommandExecutor Executor { get; } = new(context, logger);

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