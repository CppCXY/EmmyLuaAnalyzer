using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.InlineValue;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.InlineValues;

// ReSharper disable once ClassNeverInstantiated.Global
public class InlineValuesHandler(ServerContext context): InlineValueHandlerBase
{
    private InlineValuesBuilder Builder { get; } = new();
    
    protected override Task<InlineValueResponse> Handle(InlineValueParams inlineValueParams, CancellationToken cancellationToken)
    {
        var uri = inlineValueParams.TextDocument.Uri.Uri.AbsoluteUri;
        InlineValueResponse? container = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var result =  Builder.Build(semanticModel, inlineValueParams.Range);
                container = new InlineValueResponse(result);
            }
        });
        
        return Task.FromResult(container)!;
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.InlineValuesProvider = true;
    }
}