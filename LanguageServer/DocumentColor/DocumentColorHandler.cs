using EmmyLua.CodeAnalysis.Workspace;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.DocumentColor;

// ReSharper disable once ClassNeverInstantiated.Global
public class DocumentColorHandler(LuaWorkspace workspace) : DocumentColorHandlerBase
{
    private DocumentColorBuilder Builder { get; } = new();
    
    protected override DocumentColorRegistrationOptions CreateRegistrationOptions(ColorProviderCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new()
        {
            DocumentSelector = new TextDocumentSelector(new TextDocumentFilter()
            {
                Pattern = "**/*.lua"
            })
        };
    }

    public override Task<Container<ColorInformation>?> Handle(DocumentColorParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
        var semanticModel = workspace.Compilation.GetSemanticModel(uri);
        if (semanticModel is not null)
        {
            var result = Builder.Build(semanticModel);
            return Task.FromResult<Container<ColorInformation>?>(new Container<ColorInformation>(result));
        }

        return Task.FromResult<Container<ColorInformation>?>(null);
    }
}