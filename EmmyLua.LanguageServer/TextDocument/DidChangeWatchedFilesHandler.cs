using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.Registration;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceWatchedFile;
using EmmyLua.LanguageServer.Framework.Protocol.Message.WorkspaceWatchedFile.Watch;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.TextDocument;

// ReSharper disable once ClassNeverInstantiated.Global
public class DidChangeWatchedFilesHandler(ServerContext context)
    : DidChangeWatchedFilesHandlerBase
{
    private Task UpdateOneFileEventAsync(FileEvent fileEvent, CancellationToken cancellationToken)
    {
        switch (fileEvent.Type)
        {
            case FileChangeType.Created:
            case FileChangeType.Changed:
            {
                try
                {
                    var fileSystemPath = fileEvent.Uri.FileSystemPath;
                    if (context.LuaWorkspace.IsExclude(fileSystemPath))
                    {
                        return Task.CompletedTask;
                    }
                    
                    var fileText = context.LuaWorkspace.ReadFile(fileEvent.Uri.FileSystemPath);
                    var uri = fileEvent.Uri.UnescapeUri;
                    context.UpdateDocument(uri, fileText, cancellationToken);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);
                }

                break;
            }
            case FileChangeType.Deleted:
            {
                var uri = fileEvent.Uri.UnescapeUri;
                context.RemoveDocument(uri);
                break;
            }
        }

        return Task.CompletedTask;
    }

    private Task UpdateManyFileEventAsync(List<FileEvent> fileEvents,
        CancellationToken cancellationToken)
    {
        context.UpdateManyDocuments(fileEvents, cancellationToken);
        return Task.CompletedTask;
    }

    protected override async Task Handle(DidChangeWatchedFilesParams request, CancellationToken token)
    {
        var changes = request.Changes.ToList();
        if (changes.Count == 1)
        {
            await UpdateOneFileEventAsync(changes[0], token);
        }
        else
        {
            await UpdateManyFileEventAsync(changes, token);
        }
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
    }

    public override void RegisterDynamicCapability(Framework.Server.LanguageServer server,
        ClientCapabilities clientCapabilities)
    {
        var dynamicRegistration = new DidChangeWatchedFilesRegistrationOptions()
        {
            Watchers =
            [
                new()
                {
                    GlobalPattern = "**/*.lua",
                    Kind = WatchKind.Create | WatchKind.Change | WatchKind.Delete
                }
            ]
        };

        server.Client.DynamicRegisterCapability(new RegistrationParams()
        {
            Registrations =
            [
                new Registration()
                {
                    Id = Guid.NewGuid().ToString(),
                    Method = "workspace/didChangeWatchedFiles",
                    RegisterOptions = dynamicRegistration
                }
            ]
        });
    }
}