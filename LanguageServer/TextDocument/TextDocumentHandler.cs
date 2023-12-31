﻿using System.Diagnostics;
using EmmyLua.CodeAnalysis.Workspace;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using LuaDiagnostic = EmmyLua.CodeAnalysis.Compile.Diagnostic.Diagnostic;

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

public static class DiagnosticExtensions
{
    public static OmniSharp.Extensions.LanguageServer.Protocol.Models.Diagnostic ToLspDiagnostic(
        this LuaDiagnostic diagnostic, LuaDocument document)
    {
        return new()
        {
            Code = diagnostic.Code.ToString(),
            Message = diagnostic.Message,
            Range = new()
            {
                Start = new Position()
                {
                    Line = document.GetLine(diagnostic.Range.StartOffset),
                    Character = document.GetCol(diagnostic.Range.StartOffset)
                },
                End = new Position()
                {
                    Line = document.GetLine(diagnostic.Range.EndOffset),
                    Character = document.GetCol(diagnostic.Range.EndOffset)
                }
            },
            Severity = diagnostic.Severity switch
            {
                EmmyLua.CodeAnalysis.Compile.Diagnostic.DiagnosticSeverity.Error => DiagnosticSeverity.Error,
                EmmyLua.CodeAnalysis.Compile.Diagnostic.DiagnosticSeverity.Warning => DiagnosticSeverity.Warning,
                EmmyLua.CodeAnalysis.Compile.Diagnostic.DiagnosticSeverity.Information =>
                    DiagnosticSeverity.Information,
                EmmyLua.CodeAnalysis.Compile.Diagnostic.DiagnosticSeverity.Hint => DiagnosticSeverity.Hint,
                _ => throw new UnreachableException()
            },
            Source = "EmmyLua"
        };
    }
}