using EmmyLua.CodeAnalysis.Workspace;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.DocumentSymbol;

// ReSharper disable once ClassNeverInstantiated.Global
public class DocumentSymbolHandler(LuaWorkspace workspace) : DocumentSymbolHandlerBase
{
    private DocumentSymbolBuilder Builder { get; } = new();

    protected override DocumentSymbolRegistrationOptions CreateRegistrationOptions(DocumentSymbolCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new()
        {
            Label = "EmmyLua",
            DocumentSelector = new TextDocumentSelector
            (
                new TextDocumentFilter()
                {
                    Pattern = "**/*.lua"
                }
            )
        };
    }

    public override Task<SymbolInformationOrDocumentSymbolContainer?> Handle(DocumentSymbolParams request,
        CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
        var semanticModel = workspace.Compilation.GetSemanticModel(uri);
        if (semanticModel is not null)
        {
            var symbols = Builder.Build(semanticModel);
            return Task.FromResult<SymbolInformationOrDocumentSymbolContainer?>(
                SymbolInformationOrDocumentSymbolContainer.From(symbols.Select(
                    it => new SymbolInformationOrDocumentSymbol(it)))
            );
        }

        return Task.FromResult<SymbolInformationOrDocumentSymbolContainer?>(null);
    }
}