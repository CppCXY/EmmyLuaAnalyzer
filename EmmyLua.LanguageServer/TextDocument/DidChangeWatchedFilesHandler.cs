using EmmyLua.LanguageServer.Server;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using Serilog;

namespace EmmyLua.LanguageServer.TextDocument;

// ReSharper disable once ClassNeverInstantiated.Global
public class DidChangeWatchedFilesHandler(ServerContext context)
    : IDidChangeWatchedFilesHandler
{
    public DidChangeWatchedFilesRegistrationOptions GetRegistrationOptions() => new();

    public async Task<Unit> Handle(DidChangeWatchedFilesParams request, CancellationToken cancellationToken)
    {
        var changes = request.Changes.ToList();
        if (changes.Count == 1)
        {
            return await UpdateOneFileEventAsync(changes[0], cancellationToken);
        }
        else
        {
            return await UpdateManyFileEventAsync(changes, cancellationToken);
        }
    }

    private async Task<Unit> UpdateOneFileEventAsync(FileEvent fileEvent, CancellationToken cancellationToken)
    {
        switch (fileEvent.Type)
        {
            case FileChangeType.Created:
            case FileChangeType.Changed:
            {
                try
                {
                    var fileText = await File.ReadAllTextAsync(fileEvent.Uri.GetFileSystemPath(), cancellationToken);
                    var uri = fileEvent.Uri.ToUri().AbsoluteUri;
                    await context.UpdateDocumentAsync(uri, fileText, cancellationToken);
                }
                catch (Exception e)
                {
                    Log.Logger.Error(e.Message);
                }

                break;
            }
            case FileChangeType.Deleted:
            {
                var uri = fileEvent.Uri.ToUri().AbsoluteUri;
                context.RemoveDocument(uri);
                break;
            }
        }

        return Unit.Value;
    }

    private async Task<Unit> UpdateManyFileEventAsync(List<FileEvent> fileEvents,
        CancellationToken cancellationToken)
    {
        await context.UpdateManyDocumentsAsync(fileEvents, cancellationToken);
        return Unit.Value;
    }

    public DidChangeWatchedFilesRegistrationOptions GetRegistrationOptions(DidChangeWatchedFilesCapability capability,
        ClientCapabilities clientCapabilities) => new();
}