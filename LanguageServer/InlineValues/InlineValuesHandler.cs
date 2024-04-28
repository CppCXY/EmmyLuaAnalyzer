using LanguageServer.Server;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.InlineValues;

// ReSharper disable once ClassNeverInstantiated.Global
public class InlineValuesHandler(ServerContext context): InlineValuesHandlerBase
{
    private InlineValuesBuilder Builder { get; } = new();
    
    protected override InlineValueRegistrationOptions CreateRegistrationOptions(InlineValueClientCapabilities capability,
        ClientCapabilities clientCapabilities)
    {
        return new InlineValueRegistrationOptions
        {
            DocumentSelector = ToSelector.ToTextDocumentSelector(context.LuaWorkspace)
        };
    }

    public override Task<Container<InlineValueBase>?> Handle(InlineValueParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUri().AbsoluteUri;
        Container<InlineValueBase>? container = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var result =  Builder.Build(semanticModel, request.Range, request.Context);
                container = new Container<InlineValueBase>(result);
            }
        });
        
        return Task.FromResult<Container<InlineValueBase>?>(container);
    }
}