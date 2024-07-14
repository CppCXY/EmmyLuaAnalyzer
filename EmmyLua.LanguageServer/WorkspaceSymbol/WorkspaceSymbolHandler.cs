using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceSymbol;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;


namespace EmmyLua.LanguageServer.WorkspaceSymbol;

// ReSharper disable once ClassNeverInstantiated.Global
public class WorkspaceSymbolHandler(ServerContext context) : WorkspaceSymbolHandlerBase
{
    private WorkspaceSymbolBuilder Builder { get; } = new();

    protected override Task<WorkspaceSymbolResponse> Handle(WorkspaceSymbolParams request, CancellationToken token)
    {
        WorkspaceSymbolResponse? workspaceSymbols = null;
        context.ReadyRead(() =>
        {
            workspaceSymbols = new WorkspaceSymbolResponse(Builder.Build(request.Query, context, token));
        });

        return Task.FromResult(workspaceSymbols)!;
    }

    protected override Task<Framework.Protocol.Message.WorkspaceSymbol.WorkspaceSymbol> Resolve(
        Framework.Protocol.Message.WorkspaceSymbol.WorkspaceSymbol request, CancellationToken token)
    {
        return Task.FromResult(request)!;
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
        serverCapabilities.WorkspaceSymbolProvider = true;
    }
}