using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.Diagnostic;
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
    LuaWorkspace workspace,
    ILanguageServerFacade languageServerFacade
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

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(
        TextSynchronizationCapability capability,
        ClientCapabilities clientCapabilities)
        => new()
        {
            DocumentSelector = TextDocumentSelector,
            Change = Change,
            Save = new SaveOptions() { IncludeText = false }
        };

    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        workspace.UpdateDocument(request.TextDocument.Uri.ToUnencodedString(), request.TextDocument.Text);
        PushDiagnostic(request.TextDocument, workspace.GetDocument(request.TextDocument.Uri.ToUnencodedString())!);
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        var changes = request.ContentChanges.ToList();
        workspace.UpdateDocument(request.TextDocument.Uri.ToUnencodedString(), changes[0].Text);
        PushDiagnostic(request.TextDocument, workspace.GetDocument(request.TextDocument.Uri.ToUnencodedString())!);
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        workspace.CloseDocument(request.TextDocument.Uri.ToUnencodedString());
        return Unit.Task;
    }

    public void PushDiagnostic(TextDocumentIdentifier identifier, LuaDocument document)
    {
        var diagnostics = workspace.Compilation.GetDiagnostic(document.Id).Select(it => it.ToLspDiagnostic(document))
            .ToList();

        languageServerFacade.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams()
        {
            Diagnostics = Container.From(diagnostics),
            Uri = identifier.Uri,
        });
    }
}