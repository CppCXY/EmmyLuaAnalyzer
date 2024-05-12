using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Util;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;


namespace EmmyLua.LanguageServer.TextDocument;

// ReSharper disable once ClassNeverInstantiated.Global
public class TextDocumentHandler(
    ServerContext context,
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
            DocumentSelector = ToSelector.ToTextDocumentSelector(context.LuaWorkspace),
            Change = Change,
            Save = new SaveOptions() { IncludeText = false }
        };

    public override Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUri().AbsoluteUri;
        context.ReadyWrite(() =>
        {
            context.LuaWorkspace.UpdateDocumentByUri(uri, request.TextDocument.Text);
            if (context.LuaWorkspace.Compilation.GetSemanticModel(uri) is { } semanticModel)
            {
                PushDiagnostic(request.TextDocument, semanticModel);
            }
        });


        return Unit.Task;
    }

    public override Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
    {
        var changes = request.ContentChanges.ToList();
        var uri = request.TextDocument.Uri.ToUri().AbsoluteUri;
        context.ReadyWrite(() =>
        {
            context.LuaWorkspace.UpdateDocumentByUri(uri, changes[0].Text);
            if (context.LuaWorkspace.Compilation.GetSemanticModel(uri) is { } semanticModel)
            {
                PushDiagnostic(request.TextDocument, semanticModel);
            }
        });

        return Unit.Task;
    }

    public override Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
    {
        return Unit.Task;
    }

    public override Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUri().AbsoluteUri;
        context.ReadyWrite(() =>
        {
            context.LuaWorkspace.CloseDocument(uri);
        });
        
        return Unit.Task;
    }

    private void PushDiagnostic(TextDocumentIdentifier identifier, SemanticModel semanticModel)
    {
        var diagnostics = semanticModel.PopDiagnostics()
            .Select(it => it.ToLspDiagnostic(semanticModel.Document))
            .ToList();

        languageServerFacade.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams()
        {
            Diagnostics = Container.From(diagnostics),
            Uri = identifier.Uri,
        });
    }
}