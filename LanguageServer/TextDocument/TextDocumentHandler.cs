using EmmyLua.CodeAnalysis.Workspace;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace LanguageServer.TextDocument;

// ReSharper disable once ClassNeverInstantiated.Global
public class TextDocumentHandler(
    ILogger<TextDocumentHandler> logger,
    ILanguageServerConfiguration configuration,
    LuaWorkspace workspace
) : TextDocumentSyncHandlerBase
{
    private TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Full;

    private TextDocumentSelector TextDocumentSelector { get; } = new TextDocumentSelector(
        new TextDocumentFilter
        {
            Pattern = "**/*.lua"
        }
    );

    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
        => new(uri, "lua");

    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        workspace.UpdateDocument(request.TextDocument.Uri.ToUnencodedString(), request.TextDocument.Text);
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        // workspace.UpdateDocument(request.TextDocument.Uri.ToUnencodedString(), request.ContentChanges[0].Text);
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(
        TextSynchronizationCapability capability,
        ClientCapabilities clientCapabilities)
        => new()
        {
            DocumentSelector = TextDocumentSelector,
            Change = Change,
            Save = new SaveOptions() { IncludeText = true }
        };
}
