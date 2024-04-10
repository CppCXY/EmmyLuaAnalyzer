using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.Server;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.DocumentColor;

// ReSharper disable once ClassNeverInstantiated.Global
public class DocumentColorHandler(ServerContext context) : DocumentColorHandlerBase
{
    private DocumentColorBuilder Builder { get; } = new();
    
    protected override DocumentColorRegistrationOptions CreateRegistrationOptions(ColorProviderCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new()
        {
            DocumentSelector = ToSelector.ToTextDocumentSelector(context.LuaWorkspace)
        };
    }

    public override Task<Container<ColorInformation>?> Handle(DocumentColorParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
        Container<ColorInformation>? container = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                container = Builder.Build(semanticModel);
            }
        });
        
        return Task.FromResult<Container<ColorInformation>?>(container);
    }
}