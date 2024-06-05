using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace EmmyLua.LanguageServer.Configuration;

public class DidChangeConfigurationHandler : DidChangeConfigurationHandlerBase
{
    public override Task<Unit> Handle(DidChangeConfigurationParams request, CancellationToken cancellationToken)
    {
        return Unit.Task;
    }
}