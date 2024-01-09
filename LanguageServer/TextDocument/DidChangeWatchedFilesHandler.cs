using EmmyLua.CodeAnalysis.Workspace;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;

namespace LanguageServer.TextDocument;

// ReSharper disable once ClassNeverInstantiated.Global
public class DidChangeWatchedFilesHandler(LuaWorkspace workspace) : IDidChangeWatchedFilesHandler
{
    public DidChangeWatchedFilesRegistrationOptions GetRegistrationOptions() => new();

    public Task<Unit> Handle(DidChangeWatchedFilesParams request, CancellationToken cancellationToken)
    {
        foreach (var fileEvent in request.Changes)
        {
            switch (fileEvent.Type)
            {
                case FileChangeType.Created:
                {
                    workspace.AddDocument(fileEvent.Uri.ToUnencodedString(), string.Empty);
                    break;
                }
                case FileChangeType.Changed:
                    break;
                case FileChangeType.Deleted:
                {
                    workspace.RemoveDocument(fileEvent.Uri.ToUnencodedString());
                    break;
                }
            }
        }

        return Unit.Task;
    }

    public DidChangeWatchedFilesRegistrationOptions GetRegistrationOptions(DidChangeWatchedFilesCapability capability,
        ClientCapabilities clientCapabilities) => new();
}