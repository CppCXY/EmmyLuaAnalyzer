using LanguageServer.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace LanguageServer.WorkspaceSymbol;

// ReSharper disable once ClassNeverInstantiated.Global
public class WorkspaceSymbolHandler(ServerContext context) : WorkspaceSymbolsHandlerBase
{
    private WorkspaceSymbolBuilder Builder { get; } = new();
    
    protected override WorkspaceSymbolRegistrationOptions CreateRegistrationOptions(WorkspaceSymbolCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new WorkspaceSymbolRegistrationOptions()
        {
            ResolveProvider = false
        };
    }

    public override Task<Container<OmniSharp.Extensions.LanguageServer.Protocol.Models.WorkspaceSymbol>?> Handle(WorkspaceSymbolParams request, CancellationToken cancellationToken)
    {
        Container<OmniSharp.Extensions.LanguageServer.Protocol.Models.WorkspaceSymbol>? workspaceSymbols = null;
        context.ReadyRead(() =>
        {
            workspaceSymbols = Builder.Build(request.Query, context, cancellationToken);
        });

        return Task.FromResult(workspaceSymbols);
    }
}