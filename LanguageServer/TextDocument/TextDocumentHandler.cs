﻿using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.Util;
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

    public override TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
        => new(uri, "lua");

    protected override TextDocumentSyncRegistrationOptions CreateRegistrationOptions(
        TextSynchronizationCapability capability,
        ClientCapabilities clientCapabilities)
        => new()
        {
            DocumentSelector = ToSelector.ToTextDocumentSelector(workspace),
            Change = Change,
            Save = new SaveOptions() { IncludeText = false }
        };

    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
        var document = workspace.GetDocumentByUri(uri);
        if (document is not null && string.Equals(document.Text, request.TextDocument.Text, StringComparison.Ordinal))
        {
            return Unit.Task;
        }
        workspace.UpdateDocumentByUri(uri, request.TextDocument.Text);
        PushDiagnostic(request.TextDocument, workspace.Compilation.GetSemanticModel(uri)!);
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        var changes = request.ContentChanges.ToList();
        var uri = request.TextDocument.Uri.ToUnencodedString();
        workspace.UpdateDocumentByUri(uri, changes[0].Text);
        PushDiagnostic(request.TextDocument, workspace.Compilation.GetSemanticModel(uri)!);
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

    private void PushDiagnostic(TextDocumentIdentifier identifier, SemanticModel semanticModel)
    {
        var diagnostics = semanticModel.GetDiagnostic()
            .Select(it => it.ToLspDiagnostic(semanticModel.Document))
            .ToList();

        languageServerFacade.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams()
        {
            Diagnostics = Container.From(diagnostics),
            Uri = identifier.Uri,
        });
    }
    
}
