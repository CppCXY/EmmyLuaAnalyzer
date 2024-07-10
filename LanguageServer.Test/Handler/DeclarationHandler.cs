using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Declaration;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using Range = EmmyLua.LanguageServer.Framework.Protocol.Model.Range;

namespace EmmyLua.LanguageServer.Framework.Handler;

public class DeclarationHandler : DeclarationHandlerBase
{
    protected override Task<DeclarationResponse?> Handle(DeclarationParams request, CancellationToken cancellationToken)
    {
        Console.Error.WriteLine($"DeclarationHandler: Declaration {request.TextDocument.Uri}");
        return Task.FromResult(new DeclarationResponse(new Location(request.TextDocument.Uri,
            new Range(
                new Position(0, 0),
                new Position(0, 1)
            )
        )))!;
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
        serverCapabilities.DeclarationProvider = true;
    }
}