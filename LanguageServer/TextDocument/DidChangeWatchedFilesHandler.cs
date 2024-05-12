using LanguageServer.Server;
using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Workspace;
using Serilog;

namespace LanguageServer.TextDocument;

// ReSharper disable once ClassNeverInstantiated.Global
public class DidChangeWatchedFilesHandler(ServerContext context)
    : IDidChangeWatchedFilesHandler
{
    public DidChangeWatchedFilesRegistrationOptions GetRegistrationOptions() => new();

    public Task<Unit> Handle(DidChangeWatchedFilesParams request, CancellationToken cancellationToken)
    {
        context.ReadyWrite(() =>
        {
            foreach (var fileEvent in request.Changes)
            {
                switch (fileEvent.Type)
                {
                    case FileChangeType.Created:
                    {
                        try
                        {
                            var fileText = File.ReadAllText(fileEvent.Uri.GetFileSystemPath());
                            context.LuaWorkspace.UpdateDocumentByUri(fileEvent.Uri.ToUnencodedString(), fileText);
                        }
                        catch(Exception e)
                        {
                            Log.Logger.Error(e.Message);
                        }
                        break;
                    }
                    case FileChangeType.Changed:
                        break;
                    case FileChangeType.Deleted:
                    {
                        context.LuaWorkspace.RemoveDocumentByUri(fileEvent.Uri.ToUnencodedString());
                        break;
                    }
                }
            }
        });


        return Unit.Task;
    }

    public DidChangeWatchedFilesRegistrationOptions GetRegistrationOptions(DidChangeWatchedFilesCapability capability,
        ClientCapabilities clientCapabilities) => new();
}